using System.Collections.Generic;
using Squla.Core.IOC;
using Squla.Core.Logging;
using Squla.Core.Metrics;
using UnityEngine;
using WebSocketSharp;


namespace Squla.Core.Network
{
	internal class ConnectStrategy_Silent: IConnectStrategy
	{
		private readonly ScreenHUD screenHUD;

		private readonly SmartLogger logger = SmartLogger.GetLogger<ConnectStrategy_Silent>();

		public WebSocketManager_Impl context;

		private readonly IMetricManager metricManager;
		private readonly IDictionary<string, string> eventData;

		[Inject]
		public ConnectStrategy_Silent (ScreenHUD screenHUD, IMetricManager metricManager)
		{
			this.screenHUD = screenHUD;
			this.metricManager = metricManager;

			eventData = new Dictionary<string, string> {
				{"event_name", "websocket-connect-silent"}
			};
		}

		public bool MayConnect(NetworkReachability current)
		{
			return current != NetworkReachability.NotReachable && context.Websocket == null;
		}

		public void Reset()
		{
		}

		public void Connect ()
		{
			var websocket = context.Websocket;
			if (websocket.ReadyState == WebSocketState.Open)
				return;

			logger.Debug ("ConnectSilently {0}", websocket.ReadyState);
			websocket.ConnectAsync ();
			metricManager.LogEvent (eventData);
		}

		public void OnConnected ()
		{
		}

		public void GotWelcome ()
		{
		}

		public void OnError (int errorCode, bool attemptReconnect)
		{
			screenHUD.EndTransactionIfContains (WebSocketSignals.txn_WssClient_Connect);
		}
	}
}
