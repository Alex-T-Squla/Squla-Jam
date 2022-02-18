using System;
using System.Collections.Generic;

namespace Squla.Core.Logging
{
	public sealed class SmartLogger
	{
		private string name;
		private LoggerImpl impl;

		private static Dictionary<string, SmartLogger> loggerMap;

		static SmartLogger ()
		{
			loggerMap = new Dictionary<string, SmartLogger> ();
		}

		private SmartLogger (string name, LoggerImpl impl)
		{
			this.name = name;
			this.impl = impl;
		}

		public static SmartLogger GetLogger<T> ()
		{
			string name = typeof(T).FullName;
			return GetLogger (name);
		}

		public static SmartLogger GetLogger (string name)
		{
			if (!loggerMap.ContainsKey (name)) {
				var impl = SmartLogManager.GetLogger (name);
				var logger = new SmartLogger (name, impl);
				loggerMap [name] = logger;
			}

			return loggerMap [name];
		}

		public bool IsDebugLevel
		{
			get { return impl.level == LogLevel.Debug; }
		}

		public void Debug (string msg, params object[] args)
		{
			if (LogLevel.Debug >= impl.level)
				impl.Log (name, LogLevel.Debug, string.Format (msg, args));
		}

		public void Debug (bool msg, params object[] args)
		{
			Debug(msg.ToString());
		}

		public void Debug (int msg, params object[] args)
		{
			Debug(msg.ToString());
		}

		public void Debug (float msg, params object[] args)
		{
			Debug(msg.ToString());
		}

		public void Info (string msg, params object[] args)
		{
			if (LogLevel.Info >= impl.level)
				impl.Log (name, LogLevel.Info, string.Format (msg, args));
		}

		public void Warning (string msg, params object[] args)
		{
			if (LogLevel.Warning >= impl.level)
				impl.Log (name, LogLevel.Warning, string.Format (msg, args));
		}

		public void Error (string msg, params object[] args)
		{
			if (LogLevel.Error >= impl.level)
				impl.Log (name, LogLevel.Error, string.Format (msg, args));
		}
	}
}

