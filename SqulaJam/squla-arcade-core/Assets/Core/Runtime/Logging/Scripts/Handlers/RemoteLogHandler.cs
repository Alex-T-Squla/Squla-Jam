using UnityEngine;
using System;
using System.Collections;

namespace Squla.Core.Logging
{
	public class RemoteLogHandler : AbstractHandler
	{
		private LoggingWebSocketClient remoteLoggingClient;

		public RemoteLogHandler (LoggingWebSocketClient remoteLoggingClient)
		{
			this.remoteLoggingClient = remoteLoggingClient;

			Application.logMessageReceived += (condition, stackTrace, type) => {
				if (type == LogType.Exception && stackTrace.Split (new string[] { Environment.NewLine }, StringSplitOptions.None).Length > 0) {
					Log (stackTrace.Split (new string[] { Environment.NewLine }, StringSplitOptions.None) [0], LogLevel.Exception, condition);
				}
			};
		}

		public override void Log (string name, LogLevel level, string msg)
		{
			remoteLoggingClient.Log (
				string.Format ("{{\"datetime\": \"{0}\", \"name\": \"{1}\", \"level\":\"{2}\", \"msg\":\"{3}\", \"type\":\"log-message\"}}", 
					DateTime.Now.ToString ("MM/dd/yyyy hh:mm:ss.ffff"), name, level, msg.Replace ("\"", "\\\"").Replace ("\n", "")));
		}
	}
}
