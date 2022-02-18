using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Squla.Core.Logging
{
	internal class LoggerImpl
	{
		private List<AbstractHandler> handlers;
		private Dictionary<string, LoggerImpl> childLoggers = new Dictionary<string, LoggerImpl> ();

		public string name { get; private set; }

		public string short_name { get; private set; }

		public LogLevel level { get; internal set; }

		public int depth { get; private set; }

		public LoggerImpl (string name, string short_name, LogLevel level, int depth)
		{
			this.name = name;
			this.short_name = short_name;
			this.level = level;
			this.depth = depth;
		}

		public void Add (AbstractHandler handler)
		{
			if (handlers == null) {
				handlers = new List<AbstractHandler> ();
			}
			handlers.Add (handler);
		}

		public void AddLogger (LoggerImpl logger)
		{
			// if child didn't specify the hanlders,
			// then use it from parent.
			if (logger.handlers == null)
				logger.handlers = handlers;

			if (!childLoggers.ContainsKey (logger.short_name))
				childLoggers.Add (logger.short_name, logger);
		}

		internal LoggerImpl FindLogger (int index, string[] names)
		{
			if (index == names.Length)
				return this;

			var short_name = names [index];
			LoggerImpl logger = null;

			if (childLoggers.TryGetValue (short_name, out logger))
				return logger.FindLogger (index + 1, names);

			return this;
		}

		internal void FixChildrenLogLevel ()
		{
			foreach (var item in childLoggers) {
				var lg = item.Value;
				if (level < lg.level) {
					lg.level = level;
					lg.FixChildrenLogLevel ();
				}
			}
		}

		public void Log (string name, LogLevel level, string msg)
		{
			for (int i = 0; i < handlers.Count; i++) {
				if (msg.Length > 10000) {
					for (int j = 0; j < msg.Length; j += 10000) {
						handlers[i].Log(name, level, msg.Substring(j, Math.Min(10000, msg.Length-j)));
					}
				} else {
					handlers [i].Log (name, level, msg);
				}
			}
		}
	}
}

