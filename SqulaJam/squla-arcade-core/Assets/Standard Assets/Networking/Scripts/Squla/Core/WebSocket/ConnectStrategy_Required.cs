using System.Collections.Generic;
using Squla.Core.IOC;
using Squla.Core.Logging;
using WebSocketSharp;
using SimpleJson;
using Squla.Core.Metrics;
using Squla.Core.ZeroQ;
using UnityEngine;

namespace Squla.Core.Network
{
	/// <summary>
	/// before prompting the error message.
	/// </summary>
	internal class ConnectStrategy_Required: IConnectStrategy
	{
		const string txn_WssClient_Connect_Required = "txn://web-socket/request";

		private const int MAX_TRIES_PER_REQUEST = 3;

		private readonly SmartLogger logger = SmartLogger.GetLogger<ConnectStrategy_Required>();

		private readonly Bus bus;
		
		private readonly ScreenHUD screenHUD;
		private readonly IScreenHUD_UI hudUX;

		private readonly IMetricManager metricManager;
		private readonly IDictionary<string, string> eventData;

		public WebSocketManager_Impl context;

		private int tryCounter;
		
		[Inject]
		public ConnectStrategy_Required (Bus bus, ScreenHUD screenHUD, IScreenHUD_UI hudUX, IMetricManager metricManager)
		{
			this.bus = bus;
			this.screenHUD = screenHUD;
			this.hudUX = hudUX;
			this.metricManager = metricManager;

			hudUX.Retry += OnRetry;

			eventData = new Dictionary<string, string> {
				{"event_name", "websocket-connect-required"}
			};
		}

		public bool MayConnect(NetworkReachability current)
		{
			return context.Websocket == null || context.Websocket.ReadyState != WebSocketState.Open;
		}

		public void Reset()
		{
			OnConnected();
		}
		
		public void Connect ()
		{
			var websocket = context.Websocket;
			if (websocket == null || websocket.ReadyState == WebSocketState.Open)
				return;

			screenHUD.BeginTransaction(txn_WssClient_Connect_Required, forgive:true);

			logger.Debug ("ConnectRequired");
			websocket.ConnectAsync ();
			metricManager.LogEvent (eventData);
		}

		public void OnConnected ()
		{
			tryCounter = 0;
		}

		public void GotWelcome ()
		{
			screenHUD.SetIsFirstEnabled(true);
			screenHUD.EndTransactionIfContains(txn_WssClient_Connect_Required);
			
			// We call this in case there was an error beforehand
			hudUX.ChangeToErroredCancel();
		}
		
		/// <summary>
		/// Attempt to handle the NetworkRequestError.
		/// 
		/// This code is only executed for the situation where wssNecessary is true.
		/// This isn't a pure Request, so we need to create a new RequestDelegate first.
		/// This will cause the wss to attempt to reconnect one more time before further error handling occurs.
		/// 
		/// For server side errors we allow MAX_TRIES_PER_REQUEST to occur before displaying an error dialogue to the user. This is handled by 
		/// a coroutine, with rety attempts made every MAX_SECONDS_PER_NETWORK_REQUEST_RETRY seconds. 
		/// tryCounter is reset to zero each time ConnectNetworkRequest() is called. 
		/// 
		/// For internet connectivity errors or when the retry attempts equals MAX_TRIES_PER_REQUEST we display an error dialogue to the user.
		/// </summary>
		/// <param name="errorCode">Error code.</param>
		/// <param name="attemptReconnect">If set to <c>true</c> attempt reconnect.</param>
		public void OnError (int errorCode, bool attemptReconnect)
		{
			if (errorCode == -1)
				return;

			tryCounter++;

			if (!attemptReconnect || tryCounter == MAX_TRIES_PER_REQUEST) {
				logger.Debug ("DisplayNetworkRequestError");
				screenHUD.SetIsFirstEnabled(false);
				var errorResp = SimpleJSON.SerializeObject (new ErrorResponse{ error = new ErrorLite{ type = "", message = "" } });
				hudUX.ChangeToErrored(errorCode, SimpleJSON.DeserializeObject<JsonObject> (errorResp));
				return;
			}

			if (tryCounter == 1)
				bus.Publish(WebSocketSignals.cmd_WebSocket_Disconnected);
		}

		private void OnRetry()
		{
			if (screenHUD.HasRunningTransaction(txn_WssClient_Connect_Required)) {
				Connect();
			}
		}
	}
}
