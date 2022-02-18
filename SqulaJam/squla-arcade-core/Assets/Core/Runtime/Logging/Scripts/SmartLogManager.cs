using UnityEngine;
using System;
using System.Collections.Generic;
using Squla.Core.Logging.Handlers;
using SimpleJson;

namespace Squla.Core.Logging
{
	public static class SmartLogManager
	{
		private static LoggerImpl _rootLogger;
		private static bool _configured;
		private static UnityLogHandler _logHandler;

		static SmartLogManager ()
		{
			_rootLogger = new LoggerImpl ("", "", LogLevel.Error, 0);
			_logHandler = new UnityLogHandler();
			_logHandler.logMetaData = new DummyLogMetaData();
			
			_rootLogger.Add (_logHandler);
		}

		public static void SetLogMeta(ILogMeta meta)
		{
			_logHandler.logMetaData = meta;
		}
		
		public static void Configure (string name, LogLevel level, params string[] handlers)
		{
			LoggerImpl logger;

			var names = name.Split ('.');

			// find the parent logger.
			var parent = _rootLogger.FindLogger (0, names);
			if (parent.name.Equals (name)) {
				logger = parent;
				logger.level = level;
				if (!_configured)
					parent.FixChildrenLogLevel ();
				return;
			} 

			for (int depth = parent.depth; depth < names.Length; depth++) {
				var localLevel = (depth == names.Length - 1) ? level : parent.level;
				logger = new LoggerImpl (name, names [depth], localLevel, depth + 1);
				parent.AddLogger (logger);
				parent = logger;
			}
		}

		public static void Configured ()
		{
			_configured = true;
		}

		public static void SetRemoteLogging (LoggingWebSocketClient client)
		{
			_rootLogger.Add (new RemoteLogHandler (client));
		}

		internal static LoggerImpl GetLogger (string name)
		{
			if (!_configured) {
				Configure (name, LogLevel.Error);
			}

			return _rootLogger.FindLogger (0, name.Split ('.'));
		}
	}
}

