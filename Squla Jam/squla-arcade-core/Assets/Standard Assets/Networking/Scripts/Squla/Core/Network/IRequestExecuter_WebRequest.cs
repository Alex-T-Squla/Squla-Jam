using System;
using System.Collections;
using UnityEngine;
using Squla.Core.IOC;
using Squla.Core.Logging;
using Squla.Core.i18n;
using Squla.Core.Metrics;
using UnityEngine.Networking;

namespace Squla.Core.Network
{
	/// <summary>
	/// No [Sigleton] for this class.
	/// </summary>
	public class IRequestExecutor_WebRequest: IRequestExecuter
	{
		private readonly SmartLogger logger;

		private readonly ICoroutineManager manager;
		private readonly IMetricManager metricManager;
		private readonly IDataResponse nullResponse = new IDataResponse_Null (0);

		private readonly bool timeoutEnabled;

		[Inject]
		public IRequestExecutor_WebRequest (ICoroutineManager manager, IMetricManager metricManager, ILocaleConfiguration localeConfig)
		{
			logger = SmartLogger.GetLogger<IRequestExecutor_WebRequest> ();
			this.manager = manager;
			this.metricManager = metricManager;
			timeoutEnabled = localeConfig.TimeoutEnabled;
		}

		public void Execute (DataRequest request)
		{
			manager.CreateCoroutine (Worker (request, 3));
		}

		private IEnumerator Worker (DataRequest request, int maxAttempts)
		{
			var lastAttempt = maxAttempts - 1;
			var response = nullResponse;

			for (var i = 0; i < maxAttempts; i++) {
				request.tryCounter++;
				request.Status = RequestStatus.Downloading;

				var metric = metricManager.CreateNetworkMetric(request.Method, request.url, request.tryCounter);

				logger.Debug ("Downloading {0} {1} try: {2}", request.Method, request.url, request.tryCounter);
				var www = new UnityWebRequest(request.url, request.Method) {
					useHttpContinue = false,
					redirectLimit = 0,
					chunkedTransfer = false,
					downloadHandler = createDownloadHandler(request)
				};
				if (request.formData != null) {
					www.uploadHandler = new UploadHandlerRaw(request.formData) {
						contentType = request.headers.ContainsKey("Content-Type")
							? request.headers["Content-Type"]
							: "application/x-www-form-urlencoded"
					};
				} else {
					if (timeoutEnabled && lastAttempt != i) {
						// set timeout for GET requests only.
						var initialTimeout = request.requestType == DataRequest.RequestType.Json ? 10 : 5;
						www.timeout = initialTimeout + i * 5; // 10, 15, 20 seconds;
					}
				}

				foreach (var header in request.headers)
					www.SetRequestHeader(header.Key, header.Value);

				yield return www.SendWebRequest();

				var responseCode = ProcessStatusCode(www);
				var requestTime = www.GetResponseHeader ("X-Request-Time");
				if (string.IsNullOrEmpty(requestTime)) {
					metric.Response(responseCode, (int)www.downloadedBytes);
				} else {
					var respTime = 0;
					int.TryParse (requestTime, out respTime);
					metric.Response (responseCode, (int)www.downloadedBytes, respTime);
				}

				if (request.Status == RequestStatus.Aborted) {
					logger.Debug ("Aborted {0} {1}", request.Method, request.url);
					yield break;
				}

				if (responseCode == 0) {
					logger.Warning ("NetworkError {0} {1} {2}", request.Method, request.url, www.error);
					request.Status = RequestStatus.Errored;
					www.Dispose ();
				} else {
					if (responseCode > 500) {
						logger.Error ("NetworkError {0} {1} {2}", responseCode, request.Method, request.url);
						request.Status = RequestStatus.Errored;
					} else {
						request.Status = RequestStatus.Ready;
					}

					response = new IDataResponse_WebRequest (www);
//					www.Dispose ();  Note: caller of the request knows whether it is using WWW. so let that dispose the www.
					break;
				}

				if (lastAttempt == i)
					break;
			}

			if (request.onComplete == null) {
				logger.Debug("DataRequest.onComplete is null for {0}", request.url);
			} else {
				request.onComplete(request, response);
			}
		}

		private static int ProcessStatusCode (UnityWebRequest www)
		{
			return www.isNetworkError ? 0 : (int)www.responseCode;
		}

		private  DownloadHandler createDownloadHandler(DataRequest request)
		{
			if (request.requestType == DataRequest.RequestType.Audio)
				return new DownloadHandlerAudioClip(request.url, AudioType.MPEG);

			return new DownloadHandlerBuffer();
		}
	}
}
