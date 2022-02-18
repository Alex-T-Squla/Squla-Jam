using UnityEngine;

namespace Squla.Core.Network
{
	internal interface IConnectStrategy
	{
		bool MayConnect(NetworkReachability current);

		void Reset();
		
		void Connect ();

		void OnConnected ();

		void GotWelcome ();

		void OnError (int errorCode, bool attemptReconnect);
	}
}
