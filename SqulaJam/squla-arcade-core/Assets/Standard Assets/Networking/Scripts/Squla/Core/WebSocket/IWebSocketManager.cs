namespace Squla.Core.Network
{
	public interface IWebsocketManager
	{
		void SetConnectionRequired (bool required);

		void Subscribe (WebSocketAction action);

		void Unsubscribe (WebSocketAction action);

		void Send (string action);
	}
}
