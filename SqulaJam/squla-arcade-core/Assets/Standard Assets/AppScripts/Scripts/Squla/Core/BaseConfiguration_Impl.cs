using UnityEngine;
using System;
using SimpleJson;

namespace Squla.Core
{
	public class BaseConfiguration_Impl: IConfiguration
	{
		public BaseConfiguration_Impl (bool isTablet)
		{
			Version = "1.0";
			BuildNumber = 1628;
			BranchName = "master";

			IsStaging = false;
			SkipPauseBehaviour = false;
			WS_TestServerUrl = string.Empty;
			HTTP_TestServerUrl = string.Empty;
			RemoteLoggingEnabled = false;
			CheckConfigFile ();

			DeviceOS = SystemInfo.operatingSystem;
			DeviceType = isTablet ? "tablet" : "phone";

			DeviceId = PlayerPrefs.GetString ("DeviceId", string.Empty);
			if (string.IsNullOrEmpty (DeviceId)) {
				DeviceId = Guid.NewGuid ().ToString ();
				PlayerPrefs.SetString ("DeviceId", DeviceId);
			}

			var g = new Guid (DeviceId);
			DeviceId_b64 = Convert.ToBase64String (g.ToByteArray ()).Replace ("+", "-").Replace ("/", "_").Substring (0, 8);

			UpdateUserAgentString ();
		}

		public string Version { get; protected set; }

		public int BuildNumber { get; protected set; }

		public string BranchName { get; protected set; }

		public bool IsStaging { get; private set; }

		public bool SkipPauseBehaviour { get; private set; }

		public bool SkipUpdateScreen { get; private set; }

		public string DeviceOS { get; private set; }

		public string DeviceType { get; private set; }

		public string DeviceId { get; private set; }

		public string DeviceId_b64 { get; private set; }

		public string UserAgent { get; private set; }

		public string WS_TestServerUrl { get; private set; }

		public string HTTP_TestServerUrl { get; private set; }

		public string ClientStoreUrl {
			get {
#if UNITY_IOS
				return "itms-apps://itunes.apple.com/app/id1014500359";
#elif UNITY_ANDROID
				return "market://details?id=nl.squla.unitab";
#else
				return "";
#endif
			}
		}

		public bool RemoteLoggingEnabled { get; private set; }

		private void CheckConfigFile ()
		{
			var jsonString = "{}";
			var jsonStringAsset = Resources.Load ("LocaleConfigurationOverride") as TextAsset;

			if (jsonStringAsset != null)
				jsonString = jsonStringAsset.text;

			var config = (JsonObject)SimpleJSON.DeserializeObject (jsonString);
			if (config.ContainsKey ("IS_STAGING")) {
				IsStaging = bool.Parse (config ["IS_STAGING"].ToString ());
			}

			#if UNITY_EDITOR
			if (config.ContainsKey ("SKIP_PAUSE_BEHAVIOUR"))
				SkipPauseBehaviour = bool.Parse (config ["SKIP_PAUSE_BEHAVIOUR"].ToString ());
			#endif

			if (config.ContainsKey ("SKIP_UPDATE_SCREEN"))
				SkipUpdateScreen = bool.Parse (config ["SKIP_UPDATE_SCREEN"].ToString ());

			if (config.ContainsKey("TEST_SERVER_URL")) {
				WS_TestServerUrl = (string)config ["TEST_SERVER_URL"];
				if (!string.IsNullOrEmpty(WS_TestServerUrl))
					HTTP_TestServerUrl = WS_TestServerUrl.Replace("ws://", "http://");
			}

			if (IsStaging && config.ContainsKey("REMOTE_LOGGING_ENABLED")) {
				var value = bool.Parse(config["REMOTE_LOGGING_ENABLED"].ToString());
				if (value && !string.IsNullOrEmpty(WS_TestServerUrl)) {
					RemoteLoggingEnabled = true;
				}
			}
		}

		protected void UpdateUserAgentString ()
		{
			UserAgent = string.Format (
				"Arcade {0} {1}/{2}; OS: {3}; Device: {4}",
				DeviceType,
				Version,
				BuildNumber,
				DeviceOS,
				SystemInfo.deviceModel
			);
		}
	}
}

