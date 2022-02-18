using UnityEngine;
using System;

namespace Squla.Core.Logging.Handlers
{
	public class UnityLogHandler : AbstractHandler
	{
		public override void Log (string name, LogLevel level, string msg)
		{
			var dt = DateTime.Now.ToString("hh:mm:ss,fff");

			switch (level) {
			case LogLevel.Error:
				{
					Debug.LogException(new Exception(String.Format("| {0} | {1} | {2}", logMetaData.GetContext(), name, msg)));
					break;
				}
			case LogLevel.Warning:
				{
					Debug.LogWarningFormat ("| {0} | {1} | {2,5} | {3}", dt, name, level, msg);
					break;
				}
			case LogLevel.Exclude:
				{
					break;
				}
			default:
				{
					Debug.Log (string.Format ("| {0} | {1} | {2,5} | {3}", dt, name, level, msg));
					break;
				}
			}
		}
	}
}

