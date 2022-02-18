using UnityEngine;
using System.Collections;
using Squla.Core.IOC;
using System;
using WebSocketSharp;
using System.Collections.Generic;
using Squla.Core;

namespace Squla.Core.Logging
{
	// TODO: There is lots of duplication here (and desired functionality missing) with WebSocketClient.cs
	// It would be better to have a common base class.
	public class LoggingWebSocketClient : MonoBehaviourV2
	{
		private const float DEFAULT_LIFE_CHECK_INTERVAL = 2.0f;

		WebSocket webSocket;

		private NetworkReachability lastRechability;

		[Inject] private IConfiguration config;
		[Inject] private SqulaURLConfiguration urlConfiguration;

		Queue<string> controlMessages = new Queue<string> ();
		Queue<string> unsentLogs = new Queue<string> ();

		public bool activeClient {
			get {
				return webSocket != null &&
				(webSocket.ReadyState == WebSocketState.Connecting ||
				webSocket.ReadyState == WebSocketState.Open);
			}
		}

		void Update ()
		{
			lock (controlMessages) {
				if (controlMessages.Count > 0) {
					logger.Info ("Control: {0}", controlMessages.Dequeue ());
				}
			}
		}

		protected override void OnDisable ()
		{
			base.OnDisable ();

			unsentLogs.Clear ();

			webSocket.CloseAsync ();
			webSocket.OnOpen -= OnOpen;
			webSocket.OnClose -= OnClose;
			webSocket.OnError -= OnError;
			webSocket.OnMessage -= OnMessage;

			StopAllCoroutines ();
		}

		public void Initialize (string wssLoggingUrl)
		{
			var url = $"{wssLoggingUrl}{urlConfiguration.logging}";

			webSocket = new WebSocket (url);

			webSocket.OnOpen += OnOpen;
			webSocket.OnClose += OnClose;
			webSocket.OnError += OnError;
			webSocket.OnMessage += OnMessage;

			webSocket.ConnectAsync ();
			StartCoroutine (LifeCheck ());
		}

		public void Log (string log)
		{
			// TODO: Check reachability of WS and queue logged messages when connection is re-established
			if (activeClient)
				webSocket.Send (log);
			else {
				unsentLogs.Enqueue (log);
			}
		}

		void ConnectByChoice ()
		{
			if (webSocket.ReadyState == WebSocketState.Connecting || webSocket.ReadyState == WebSocketState.Open)
				return;

			logger.Debug ("ConnectByChoice");
			webSocket.ConnectAsync ();
		}

		void CloseByChoice (CloseStatusCode code = CloseStatusCode.NoStatus, string reason = "")
		{
			if (webSocket.ReadyState == WebSocketState.Closing || webSocket.ReadyState == WebSocketState.Closed)
				return;

			logger.Debug ("CloseByChoice");
			webSocket.CloseAsync (code, reason);
		}

		void OnOpen (object sender, EventArgs e)
		{
			lock (controlMessages) {
				controlMessages.Enqueue (string.Format ("Socket connection opened: {0}", webSocket.Url.ToString ()));
			}

			// Send user agent info
			webSocket.Send (string.Format ("{{\"user_agent\": \"{0}\", \"type\": \"user-agent\"}}", config.UserAgent));

			lock (unsentLogs) {
				while (unsentLogs.Count > 0) {
					Log (unsentLogs.Dequeue ());
				}
			}
		}

		void OnClose (object sender, CloseEventArgs e)
		{
			lock (controlMessages)
				controlMessages.Enqueue (String.Format ("Socket connection closed: {0}:{1}", e.Code, e.Reason));
		}

		void OnError (object sender, ErrorEventArgs e)
		{
			// This is just to supress an annoying Debug.Log to be printed to the screen 
			if (e.Message.EndsWith ("This operation isn't available in: connecting"))
				return;
			
			lock (controlMessages)
				controlMessages.Enqueue (string.Format ("Socket error: {0}", e.Message));
		}

		void OnMessage (object sender, MessageEventArgs e)
		{
			lock (controlMessages)
				controlMessages.Enqueue (e.Data);
		}

		IEnumerator LifeCheck ()
		{
			lastRechability = Application.internetReachability;

			while (true) {
				var currentReachability = Application.internetReachability;

				bool rechabilityChanged = currentReachability != lastRechability;
				lastRechability = currentReachability;

				//logger.Debug ("LifeCheck {0}, {1}, {2}", websocket.ReadyState, rechabilityChanged, currentReachability);

				if (rechabilityChanged && webSocket.ReadyState == WebSocketState.Open) {
					CloseByChoice (CloseStatusCode.Abnormal, "Internet reachability problem.");
				} else if (currentReachability != NetworkReachability.NotReachable) {
					ConnectByChoice ();
				}

				yield return new WaitForSeconds (DEFAULT_LIFE_CHECK_INTERVAL);
			}
		}
	}
}