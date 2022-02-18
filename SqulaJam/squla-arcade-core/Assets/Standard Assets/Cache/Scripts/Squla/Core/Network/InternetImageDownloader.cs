using UnityEngine;
using System;
using System.Collections.Generic;
using Squla.Core.IOC;
using Squla.Core.Logging;

namespace Squla.Core.Network
{
	[Singleton]
	public class InternetImageDownloader : IFallbackImageDownloader
	{
		private readonly SmartLogger logger = SmartLogger.GetLogger<InternetImageDownloader>();
		private readonly SpriteRequestManager manager;
		private readonly List<ErroredRequest> erroredRequests = new List<ErroredRequest>();
//		private readonly IScreenHUD_UI hudUX;
		private readonly SpinnerModel spinnerModel;

		[Inject]
		public InternetImageDownloader (SpriteRequestManager manager/*, IScreenHUD_UI hudUX*/, SpinnerModel spinnerModel)
		{
			this.manager = manager;
//			this.hudUX = hudUX;
			this.spinnerModel = spinnerModel;

//			hudUX.Retry += OnHudUXRetry;
//			hudUX.Abort += OnHudUXAbort;
		}

		public void GetImage (string url, Action<string, Sprite> CallerFinished)
		{
			void NewCaller(string nestedUrl, int httpStatus, Sprite sprite)
			{
				if (httpStatus == 0) {
					erroredRequests.Add(new ErroredRequest {
						url = nestedUrl,
						callback = CallerFinished
					});
//					hudUX.ChangeToErrored(httpStatus, null);
					return;
				}

				if (httpStatus == 404 || httpStatus == 500) {
					logger.Error("NetworkError {0} GET {1}",  httpStatus, nestedUrl);
				}

				spinnerModel.End(nestedUrl);
				CallerFinished(nestedUrl, sprite);
			}

			manager.GET(url, NewCaller);
		}

		public void Flush ()
		{
			manager.Flush ();
		}

		private void OnHudUXAbort ()
		{
			logger.Debug ("Aborting {0} asset request(s)", erroredRequests.Count);
			erroredRequests.Clear();
		}

		private void OnHudUXRetry ()
		{
			logger.Debug ("Retrying {0} errored requests", erroredRequests.Count);
			var requests = erroredRequests.ToArray();
			erroredRequests.Clear();
			for (var i = 0; i < requests.Length; i++) {
				var req = requests[i];
				spinnerModel.Begin(req.url);
				GetImage(req.url, req.callback);
			}
		}

		private class ErroredRequest
		{
			public string url;
			public Action<string, Sprite> callback;
		}
	}
}
