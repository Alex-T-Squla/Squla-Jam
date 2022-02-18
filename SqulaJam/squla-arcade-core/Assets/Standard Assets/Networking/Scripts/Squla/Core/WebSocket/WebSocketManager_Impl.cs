using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Squla.Core.i18n;
using Squla.Core.IOC;
using WebSocketSharp;
using System.Text.RegularExpressions;
using System;
using Squla.Core.ZeroQ;
using SimpleJson;
using Squla.Core.Metrics;

/*
 *
 * WSS Response codes and meaning
 * 	A comprehensive overview can be found here https://tools.ietf.org/html/rfc6455#section-7.4
 *
 * 	1000 - Normal close. If the client closes the connection we send this code to the ws client
 *
 *  1002 - ProtocolError. Not a WebSocket handshake response. This occurs if the client attempts to make a connection to
 *  a wss which isn't available. We pass this onto the ScreenHUD as a http 502 error.
 *
 *  1004 - Undefined
 *
 *  1005 - server close (no status).
 *
 * 	1006 - Internet reachability problem. Note we are sending this code to the ws client when
 * 	we have an internet reachability problem. "Abnormal close"
 *
 * 	1011 - Server Error
 *
 * WSS scenarios
 *
 * 	We consider two different client states for handling client WS connections.
 *
 * 		WSS Necessary state
 * 			This state corresponds to a situation where the client is actively relying on the ws connection to proceed.
 * 			E.g. User is currently participating in a live battle.
 * 			In this state any ws connection interruptions will lead to a network error modal with only the retry button enabled.
 * 			This means the user cannot proceed in the app until the connection to the websocket is re-established.
 *
 * 			In this state all attempts to establish a websocket connection are wrapped in a NetworkRequestDelegate object.
 *
 * 			In this state if the websocket connection is interrupted / closed, we have to create a new NetworkRequestDelegate in order to
 * 			display the error dialogue. This causes one extra connect attempt to occur before displaying the error modal.
 *
 * 		WSS Not Necessary state
 * 			This state corresponds to a situation where the client is passively relying on the ws connection to proceed.
 * 			E.g. in the menu screen.
 *
 * 			The ws client is in a silent reconnect mode, relying on the LifeCheck coroutine to simply
 * 			reconnect without wrapping it in a request delegate object.
 *
 * 			If the user transitions into a WSS Necessary state from here we first check if a connection is open and if not attempt to
 * 			open a new connection wrapped in a request delegate. This ensures the user cannot proceed without a ws connection.
 *
 * 			In the silent state we do not show any error dialogue to the user if the connection fails.
 *
 * 	The app background / foregound adds more complexity to the above scenarios as the websocket connection is closed when transitioning to the background
 * 	and reopened when the app comes back into the foreground. We need to be careful to not trigger multiple WSS connection requests in this case.
 *
 */

namespace Squla.Core.Network
{
	public class WebSocketManager_Impl: MonoBehaviourV2, IWebsocketManager, IWSSWelcome
	{
		private const float DEFAULT_LIFE_CHECK_INTERVAL = 2.0f;
		private const string SOCKET_CONNECTION_OPENED_MSG = "Socket connection opened.";

		private string accessToken;
		private IWSSMessageRouter router;
		
		private WebSocket websocket;
		
		internal WebSocket Websocket {
			get { return websocket;  }
		}
		
		/// <summary>
		/// This boolean determines if the user is in a state which requires an active wss connection 
		/// </summary>
		private bool activeClient;

		/// <summary>
		/// application needs WSS connection to function.
		/// </summary>
		private bool wssNeeded;

		private static readonly Regex statusRegex = new Regex (@"(200|[2345]\d\d)", RegexOptions.Compiled);

		private readonly Queue<ControlMessage> controlMessages = new Queue<ControlMessage> ();
		private readonly Dictionary<string, WebSocketAction> actionsOnConnect = new Dictionary<string, WebSocketAction> ();

		private readonly Dictionary<string, string> disconnectEvent = new Dictionary<string, string> {
			{"event_name", "websocket-disconnect-due-to-network-change"}
		};

		[Inject]
		private IConfiguration config;

		[Inject]
		private ILocaleConfiguration localeConfig;

		[Inject]
		private IMetricManager metricManager;

		[Inject]
		private ScreenHUD screenHUD;

		[Inject]
		private IAuthService authService;

		private IConnectStrategy connectStrategy;

		[Inject]
		private ConnectStrategy_Silent silentConnectStrategy;

		[Inject]
		private ConnectStrategy_Required requiredConnectStrategy;

		protected override void AfterAwake()
		{
			silentConnectStrategy.context = this;
			requiredConnectStrategy.context = this;
		}

		public void Init (IWSSMessageRouter router)
		{
			this.router = router;

			connectStrategy = silentConnectStrategy;
			wssNeeded = false;

			router.Init (this);
		}

		#region IWSSWelcome implementation

		public void GotWelcome ()
		{
			if (websocket == null)
				return;

			foreach (var keyvalue in actionsOnConnect) {
				websocket.SendAsync (keyvalue.Value.sub, OnSent);
			}
			logger.Debug ("Got welcome");
			connectStrategy.GotWelcome ();
			screenHUD.EndTransactionIfContains(WebSocketSignals.txn_WssClient_Connect);
		}

		public void GotUnauthorized ()
		{
			// Get new access token and then close + reopen socket with new url
			logger.Debug ("Unauthorized msg received -> Try get new access token");
			WebsocketClient_Dtor ();

			authService.SetAccessTokenExpired ();
			authService.GetAccessToken (a => {
				WebsocketClient_Ctor (a.access_token);
			}, (resCode, resObj) => { 
				logger.Debug ("GetAccessToken Error: {0}:{1}", resCode, resObj);
			});
		}

		#endregion

		/// <summary>
		/// This is the method that can start first connection.
		/// This is to decouple user object from implementation.
		/// pass null accessToken to disconnect, valid accessToken to start the connection.
		/// </summary>
		/// <param name="accessToken">Access token.</param>
		public void SetAccessToken (string accessToken)
		{
			if (string.IsNullOrEmpty (localeConfig.URL_WSS))
				return;

			WebsocketClient_Dtor ();

			this.accessToken = accessToken;
			if (!string.IsNullOrEmpty (accessToken)) {
				WebsocketClient_Ctor (accessToken);
			}
		}

		[Subscribe (WebSocketSignals.cmd_WebSocket_State_Changed)]
		public void SetConnectionRequired (bool required)
		{
			logger.Debug ("Websocket state changed: {0}", wssNeeded ? "Needed" : "Not needed");
			var prevState = wssNeeded;
			wssNeeded = required;

			// This check is to ensure that we prevent multiple connection attempts.
			if (wssNeeded && prevState == wssNeeded)
				return;

			if (connectStrategy != null)
				connectStrategy.Reset();

			connectStrategy = wssNeeded ? (IConnectStrategy)requiredConnectStrategy : (IConnectStrategy)silentConnectStrategy;
		}

		[Subscribe (WebSocketSignals.cmd_Subscribe)]
		public void Subscribe (WebSocketAction action)
		{
			if (actionsOnConnect.ContainsKey (action.node_id))
				return;

			actionsOnConnect.Add (action.node_id, action);
			if (websocket != null)
				websocket.SendAsync (action.sub, OnSent);
		}

		[Subscribe (WebSocketSignals.cmd_Unsubscribe)]
		public void Unsubscribe (WebSocketAction action)
		{
			if (!actionsOnConnect.ContainsKey (action.node_id))
				return;

			actionsOnConnect.Remove (action.node_id);
			if (websocket != null)
				websocket.SendAsync (action.unsub, OnSent);
		}

		[Subscribe (WebSocketSignals.cmd_Send)]
		public void Send (string action)
		{
			if (websocket == null)
				return;

			websocket.SendAsync (action, OnSent);
		}

		private void OnSent (bool sent)
		{
			if (!sent)
				logger.Error ("sending failed");
		}

		private void WebsocketClient_Init(string accessToken)
		{
			var newURL = string.Format ("{0}/v1/node/{1}/{2}", localeConfig.URL_WSS, accessToken, config.DeviceId_b64);
			logger.Debug ("URL: {0}", newURL);
			websocket = new WebSocket (newURL);
			websocket.UserAgent = config.UserAgent;
			websocket.TcpLingerEnabled = true;
			websocket.TcpLingerTime = 5; // time in seconds

			websocket.OnOpen += Websocket_OnOpen;
			websocket.OnClose += Websocket_OnClose;
			websocket.OnError += Websocket_OnError;
			websocket.OnMessage += Websocket_OnMessage;
		}

		// Websocket client constructor
		private void WebsocketClient_Ctor (string accessToken)
		{
			WebsocketClient_Init(accessToken);
			activeClient = true;
			StartCoroutine (LifeCheck ());
		}


		private void WebsocketClient_Destroy(CloseStatusCode code, string reason)
		{
			websocket.OnOpen -= Websocket_OnOpen;
			websocket.OnClose -= Websocket_OnClose;
			websocket.OnError -= Websocket_OnError;
			websocket.OnMessage -= Websocket_OnMessage;

			logger.Debug ("CloseAsync");
			websocket.CloseAsync (code, reason);
			websocket = null;
		}

		// Websocket client destructor
		private void WebsocketClient_Dtor ()
		{
			if (websocket == null || websocket.ReadyState == WebSocketState.Closed || websocket.ReadyState == WebSocketState.Closing)
				return;

			logger.Debug ("WebsocketClient_Dtor: Closing websocket");
			WebsocketClient_Destroy(code: CloseStatusCode.Normal, reason: "Client closed connection");

			lock (controlMessages) {
				controlMessages.Clear ();
			}
			router.ClearMessages ();

			activeClient = false;
			StopAllCoroutines ();
		}

		private void Websocket_OnOpen (object sender, EventArgs e)
		{
			lock (controlMessages) {
				controlMessages.Enqueue (new ControlMessage{ msg = SOCKET_CONNECTION_OPENED_MSG });
			}
		}

		private void Websocket_OnClose (object sender, CloseEventArgs e)
		{
			lock (controlMessages) {
				var msg = string.Format ("Socket connection closed: {0}: {1}", e.Code, e.Reason);
				controlMessages.Enqueue (new ControlMessage { msg = msg, resp_code = e.Code });

				router.OnDisconnect ();
			}
		}

		private void Websocket_OnError (object sender, ErrorEventArgs e)
		{
			lock (controlMessages) {
				controlMessages.Enqueue (new ControlMessage { msg = string.Format ("Socket error: {0}", e.Message) });
			}
		}

		private void Websocket_OnMessage (object sender, MessageEventArgs e)
		{
			router.OnMessage (e.Data);
		}

		public void RouteMsg (JsonObject json)
		{
			router.Route (json);
		}

		private void Update ()
		{
			lock (controlMessages) {
				if (controlMessages.Count > 0) {
					var ctrlMsg = controlMessages.Dequeue ();
					logger.Info ("Control: {0}", ctrlMsg.msg);

					if (ctrlMsg.msg == SOCKET_CONNECTION_OPENED_MSG) {
						connectStrategy.OnConnected ();
					}

					// Android reports connection errors with resp. code 1005.
					// iOS reports connection errors with resp. code 1006.
					var reachable = Application.internetReachability != NetworkReachability.NotReachable;
					var connectionError = ctrlMsg.resp_code == 1006 || (ctrlMsg.resp_code == 1005 && !reachable);

					if (connectionError) {
						logger.Debug ("Websocket connection error");
						connectStrategy.OnError (0, false);
					} else if (ctrlMsg.resp_code == 1005) {
						logger.Debug ("Websocket connection closed without reason");
						connectStrategy.OnError (502, true);
					} else {
						// Try parse for http error
						var statusCode = ProcessStatusCode (ctrlMsg.msg);
						connectStrategy.OnError (statusCode, true);
					}
				}
			}

			router.Update ();
		}

		private void CloseByChoice (CloseStatusCode code = CloseStatusCode.NoStatus, string reason = "")
		{
			if (websocket == null || websocket.ReadyState == WebSocketState.Closing || websocket.ReadyState == WebSocketState.Closed)
				return;

			logger.Debug ("CloseByChoice");

			if (code == CloseStatusCode.Normal && !string.IsNullOrEmpty (router.RoomNodeId)) {
				var json = new JsonObject ();
				json.Add ("action", "app_closed");
				json.Add ("node_id", router.RoomNodeId);
				json.Add ("msg", new JsonObject ());
				websocket.Send (json.ToString ());
			}

			WebsocketClient_Destroy(code, reason);
			router.OnDisconnect();
		}

		#if UNITY_EDITOR
		private void OnDestroy ()
		{
			if (websocket != null) {
				logger.Info ("OnDestroy close");
				WebsocketClient_Destroy(code: CloseStatusCode.Normal, reason: "Client closed connection");
			}

			StopAllCoroutines ();
		}
		#endif

		private void OnApplicationPause (bool pause)
		{
			// if the client is not active, then are closed by definition.
			if (!activeClient)
				return;

			if (config.SkipPauseBehaviour)
				return;

			if (pause) {
				StopAllCoroutines ();
				CloseByChoice (code: CloseStatusCode.Normal, reason: "Client closed connection");
				screenHUD.BeginTransaction (WebSocketSignals.txn_WssClient_Connect, forgive:true);
			} else {
				// resume.
				WebsocketClient_Ctor (accessToken);
			}
		}

		private IEnumerator LifeCheck ()
		{
			var lastRechability = Application.internetReachability;

			// This is just to ensure we don't connect until the websocket is properly closed.
			while (websocket.ReadyState == WebSocketState.Closing)
				yield return new WaitForEndOfFrame ();

			logger.Debug ("WebSocket - Try connection");
			connectStrategy.Connect ();

			yield return new WaitForSeconds (DEFAULT_LIFE_CHECK_INTERVAL);

			while (true) {
				var currentReachability = Application.internetReachability;
				var last = lastRechability;

				var rechabilityChanged = currentReachability != lastRechability;
				lastRechability = currentReachability;

				if (rechabilityChanged && websocket != null && websocket.ReadyState == WebSocketState.Open) {
					disconnectEvent ["network_previous"] = last.ToString ();
					disconnectEvent ["network_now"] = currentReachability.ToString ();
					metricManager.LogEvent (disconnectEvent);
					CloseByChoice (CloseStatusCode.Abnormal, "Internet reachability problem.");
				} else if (connectStrategy.MayConnect(currentReachability)) {
					if (websocket == null)
						WebsocketClient_Init(accessToken);
					connectStrategy.Connect ();
				}

				yield return new WaitForSeconds (DEFAULT_LIFE_CHECK_INTERVAL);
			}
		}

		private static int ProcessStatusCode (string resp)
		{
			var statusCode = -1;
			var match = statusRegex.Match (resp);
			if (match.Success) {
				statusCode = int.Parse (match.Groups [0].Captures [0].Value);
			}

			return statusCode;
		}
	}
}
