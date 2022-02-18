using UnityEngine;

namespace Squla.Core.Logging
{
	public class RemoteLoggingConfig : LoggingConfig
	{
		public void Initialize (IConfiguration config)
		{
			if (string.IsNullOrEmpty (config.WS_TestServerUrl))
				return;
			GameObject remoteLoggingPrefab = Resources.Load<GameObject> ("prefab_LoggingWebSocketClient");
			GameObject remoteLoggingInstance = Instantiate (remoteLoggingPrefab);
			remoteLoggingInstance.GetComponent<RectTransform> ().SetParent (gameObject.GetComponent<RectTransform> ());
			LoggingWebSocketClient loggingWebSocketClient = remoteLoggingInstance.GetComponent<LoggingWebSocketClient> ();
			loggingWebSocketClient.Initialize (config.WS_TestServerUrl);

			SmartLogManager.SetRemoteLogging (loggingWebSocketClient);
		}
	}
}
