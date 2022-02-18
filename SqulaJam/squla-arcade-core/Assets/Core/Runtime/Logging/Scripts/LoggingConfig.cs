using System;
using UnityEngine;
using SimpleJson;

namespace Squla.Core.Logging
{
	public class LoggingConfig : MonoBehaviour
	{
		[Serializable]
		public class Config
		{
			public string name;
			public LogLevel level;
		}

		public Config[] configs;

		protected virtual void Awake ()
		{
			if (configs == null || configs.Length == 0) {
				SmartLogManager.Configured ();
				return;
			}

			for (int i = 0; i < configs.Length; i++) {
				var cfg = configs [i];
				var name = cfg.name.Equals ("root") ? "" : cfg.name;
				SmartLogManager.Configure (name, cfg.level);
			}

			SmartLogManager.Configured ();
		}
	}
}
