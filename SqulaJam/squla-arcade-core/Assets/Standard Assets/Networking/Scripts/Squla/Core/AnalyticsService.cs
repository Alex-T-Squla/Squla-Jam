using UnityEngine;
using Squla.Core.IOC;
using Squla.Core.i18n;
using Squla.Core.ZeroQ;
using Squla.Core.Network;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SimpleJson;
using Squla.Core.ExtensionMethods;
using Squla.Core.Metrics;

namespace Squla.Core
{
	public class AnalyticsService : IAnalytics
	{
		private readonly IConfiguration config;
		private readonly ILocaleConfiguration localeConfig;
		private readonly JsonRequestManager requestManager;
		private readonly string version;
		private readonly Dictionary<string, string> exceptionData;
		private readonly NavigationStackV2 navStack;
		private readonly ScreenHUD screenHud;
		private string authorization;
		private string url;
		private string metricUrl;
		private string exceptionUrl;
		private string userId;

		public string metricsPath;

		[Inject]
		public AnalyticsService (
			Bus bus, 
			IConfiguration config, 
			ILocaleConfiguration localeConfig, 
			JsonRequestManager requestManager, 
			IMetricManager metricManager, 
			NavigationStackV2 navStack,
			ScreenHUD screenHud
			)
		{
			this.config = config;
			this.localeConfig = localeConfig;
			this.requestManager = requestManager;
			this.navStack = navStack;
			this.screenHud = screenHud;
			version = $"Arcade {config.Version}/{config.BuildNumber}";

			exceptionData = new Dictionary<string, string> {
				{"error", string.Empty},
				{"device_model", SystemInfo.deviceModel},
				{"os", SystemInfo.operatingSystem},
				{"memory", SystemInfo.systemMemorySize.ToString ()},
				{"build_guid", Application.buildGUID},
				{"version", config.Version},
				{"build_number", config.BuildNumber.ToString()}
			};

			bus.Register (this);
			metricManager.RegisterOnException (HandleException);
		}

		[Subscribe (SignalsAnalytics.model_Locale_Changed)]
		public void WhenLocaleChanged (SqulaLocale locale)
		{
			url = localeConfig.URL_API + "/v2/services/log-app-event";
			metricUrl = localeConfig.URL_API + "/v2/services/app-metric";
			exceptionUrl = localeConfig.URL_EXCEPTION + "/v1/notify";

			authorization = "Basic " + System.Convert.ToBase64String (System.Text.Encoding.ASCII.GetBytes (localeConfig.CLIENT_ID + ":" + localeConfig.CLIENT_SECRET));
		}

		[Subscribe (SignalsAnalytics.cmd_Log_Event)]
		public void LogEvent (IDictionary<string, string> properties)
		{
			var form = new WWWForm ();
			form.AddFields (properties);
			form.AddField ("device_id", config.DeviceId);
			form.AddField ("device_os", config.DeviceOS);
			form.AddField ("device_type", config.DeviceType);
			form.AddField ("app_version", version);
			if (!string.IsNullOrEmpty (userId))
				form.AddField ("user_id", userId);

			var request = new JsonRequest {
				url = url,
				authorization = authorization,
				background = true,
				formData = form.data,
				onComplete = (a, b, c) => true
			};
			requestManager.Execute(request);
		}

		public void SetUserInfo (string userId, string userType)
		{
			this.userId = userId;
			exceptionData["user_id"] = userId;
			exceptionData["user_type"] = userType;
		}

		public void UploadMetrics()
		{
			if (!Directory.Exists(metricsPath))
				return;

			var d = new DirectoryInfo(metricsPath);
			foreach (var f in d.GetFiles()) {

				var data = File.ReadAllBytes (f.FullName);

				// upload files
				PostMetric (f.Name, data);

				// delete files.
				f.Delete();
			}
		}

		private void PostMetric (string fileName, byte[] data)
		{
			var form = new WWWForm ();
			form.AddBinaryData ("metric", data, fileName);

			var request = new JsonRequest {
				url = metricUrl,
				authorization = authorization,
				background = true,
				formData = form.data,
				contentType = form.headers["Content-Type"],
				onComplete = (a, b, c) => true
			};
			requestManager.Execute(request);
		}

		private void HandleException (string time, string error)
		{
			exceptionData ["t"] = time;
			exceptionData ["error"] = error;
			exceptionData ["screen"] = GetNavStackPath();
			if (screenHud != null)
				exceptionData["txs"] = screenHud.GetRunningTransactionList();
			
			if (error.Contains (exceptionUrl))
				// to avoid recursive call of reporting exception.
				return;

			var jsonData = SimpleJSON.SerializeObject(exceptionData);
			var dataBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);
			
			var request = new JsonRequest {
				url = exceptionUrl,
				authorization = authorization,
				background = true,
				formData = dataBytes,
				contentType = "application/json",
				onComplete = (a, b, c) => true
			};
			requestManager.Execute(request);
		}

		private string GetNavStackPath()
		{
			if (navStack == null)
				return "Null";
			string screenPath = "None";

			if (navStack.Count > 0) {
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < navStack.Count; i++) {
					sb.Append(navStack.Peek(i - navStack.Count + 1).GameObjectName);
					if (i < navStack.Count - 1) {
						sb.Append("/");
					}
				}
				screenPath = sb.ToString();
			}
			return screenPath;
		}
	}
}
