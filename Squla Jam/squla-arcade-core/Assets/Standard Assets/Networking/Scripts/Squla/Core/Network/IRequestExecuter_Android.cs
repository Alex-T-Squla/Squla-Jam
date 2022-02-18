using System;
using System.Collections;
using UnityEngine;
using Squla.Core.IOC;
using Squla.Core.Logging;
using System.Text.RegularExpressions;
using Squla.Core.Metrics;

namespace Squla.Core.Network
{
	/// <summary>
	/// No [Sigleton] for this class.
	/// </summary>
	public class IRequestExecutor_Android: IRequestExecuter
	{
		private readonly SmartLogger logger;

		private readonly Regex statusRegex = new Regex (@"\s(200|[2345]\d\d)\s", RegexOptions.Compiled);

		private readonly ICoroutineManager manager;
		private readonly IMetricManager metricManager;
		private readonly IDataResponse nullResponse = new IDataResponse_Null (0);

		[Inject]
		public IRequestExecutor_Android (ICoroutineManager manager, IMetricManager metricManager)
		{
			logger = SmartLogger.GetLogger<IRequestExecutor_Android> ();
			this.manager = manager;
			this.metricManager = metricManager;
		}

		public void Execute (DataRequest request)
		{
			manager.CreateCoroutine (Worker (request, 3));
		}

		public IEnumerator Worker (DataRequest request, int maxAttempts)
		{
			var lastAttempt = maxAttempts - 1;
			var response = nullResponse;

			for (var i = 0; i < maxAttempts; i++) {
				request.tryCounter++;
				request.Status = RequestStatus.Downloading;

				var metric = metricManager.CreateNetworkMetric(request.Method, request.url, request.tryCounter);

				logger.Debug ("Downloading {0} {1} try: {2}", request.Method, request.url, request.tryCounter);
				var www = new WWW (request.url, request.formData, request.headers);
				yield return www;

				var responseCode = ProcessStatusCode(www);
				metric.Response(responseCode, www.bytesDownloaded);

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

					response = new IDataResponse_WWW (www, responseCode);
//					www.Dispose ();  Note: caller of the request knows whether it is using WWW. so let that dispose the www.
					break;
				}

				if (lastAttempt == i)
					break;

				var delay = (i + 2) * 1.0f;
				yield return new WaitForSeconds (delay);
			}

			if (request.onComplete == null) {
				logger.Debug("DataRequest.onComplete is null for {0}", request.url);
			} else {
				request.onComplete(request, response);
			}
		}

		private int ProcessStatusCode (WWW www)
		{
			if (www.responseHeaders == null || www.responseHeaders.Count == 0)
				return 0;

			var statusText = string.Empty;
			var hasStatus = www.responseHeaders.TryGetValue ("STATUS", out statusText);
			if (!hasStatus) {
				statusText = www.error;
			}

			var match = statusRegex.Match (statusText);
			return match.Success
				? int.Parse (
					match.Groups [0]
						.Captures [0]
						.Value)
				: 0;
		}
	}
}
