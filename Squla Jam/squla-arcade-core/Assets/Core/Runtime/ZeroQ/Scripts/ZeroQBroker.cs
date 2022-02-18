using UnityEngine;
using Squla.Core.IOC;
using Squla.Core.Logging;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Squla.Core.ZeroQ
{

	public class ZeroQBroker : MonoBehaviour
	{
		protected static SmartLogger logger = SmartLogger.GetLogger<ZeroQBroker> ();

		public CommandMap[] commandMaps;

		private Bus bus;
		private Dictionary<string, string> _commandMaps;

		void Awake ()
		{
			var graph = ObjectGraph.main;
			if (graph == null) {
				logger.Error ("ObjectGraph is not initialized.");
			} else {
				bus = graph.Get<Bus> ();
			}

			if (commandMaps != null) {
				_commandMaps = commandMaps.ToDictionary (i => i.source, i => i.target);
			}
		}

		public void SendCommand (string command)
		{
			if (bus != null) {
				bus.Publish (command, null);
			}
		}

		public void PlaySoundFX (string clipName)
		{
			bus.Publish ("cmd://audio-sfx/play", clipName);
		}

		internal void Publish (string command)
		{
			if (_commandMaps != null && _commandMaps.ContainsKey (command)) {
				var target = _commandMaps [command];
				bus.Publish (target);
			} else {
				bus.Publish (command);
			}
		}

		[Serializable]
		public class CommandMap
		{
			public string source;
			public string target;
		}
	}

}