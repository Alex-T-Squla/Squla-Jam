using SimpleJson;

namespace Squla.Core.Network
{
	public interface IWSSMessageRouter
	{
		string RoomNodeId { get; }

		void Init (IWSSWelcome target);

		void Route(JsonObject obj);

		void OnMessage (string messageData);

		void OnDisconnect ();

		void ClearMessages ();

		void Update ();
	}
}
