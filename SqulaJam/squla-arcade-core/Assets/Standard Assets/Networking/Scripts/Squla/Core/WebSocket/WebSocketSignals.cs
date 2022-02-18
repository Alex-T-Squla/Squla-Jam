namespace Squla.Core.Network
{
	public class WebSocketSignals
	{
		public const string cmd_WebSocket_State_Changed = "cmd://web-socket/foreground";
		public const string cmd_WebSocket_Got_Welcome = "cmd://web-socket/got-welcome";
		public const string cmd_WebSocket_Disconnected = "cmd://web-socket/disconnected";

		public const string cmd_Subscribe = "cmd://websocket/subscribe";
		public const string cmd_Send = "cmd://websocket/send";
		public const string cmd_Unsubscribe = "cmd://websocket/unsubscribe";

		public const string model_WebSocket_Welcome = "model://web-socket/welcome";

		public const string txn_WssClient_Connect = "txn://wss-client/connect";
	}

}
