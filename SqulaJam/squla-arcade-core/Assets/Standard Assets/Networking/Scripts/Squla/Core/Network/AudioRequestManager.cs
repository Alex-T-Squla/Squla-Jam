using System;
using UnityEngine;
using Squla.Core.IOC;
using Squla.Core.Metrics;

namespace Squla.Core.Network
{
	[Singleton]
	public class AudioRequestManager
	{
		private readonly INetworkManager manager;

		private readonly RequestMetrics metrics;

		[Inject]
		public AudioRequestManager ([Inject ("Asset")] INetworkManager networkManager, IMetricManager metricManager)
		{
			manager = networkManager;
			metrics = new RequestMetrics ("audio", metricManager);
		}

		public void GET (string audioURL, Action<string, AudioClip, byte[]> onComplete)
		{
			metrics.Request ();

			var req = new DataRequest {
				requestType = DataRequest.RequestType.Audio,
				url = audioURL,
				source = onComplete,
				onComplete = Response
			};

			manager.Execute (req);
		}

		private void Response (DataRequest req, IDataResponse resp)
		{
			metrics.Response (resp.ResponseCode);

			var onComplete = (Action<string, AudioClip, byte[]>)req.source;

			if (resp.AudioClip != null) {
				resp.AudioClip.name = req.url;
			}

			onComplete (req.url, resp.AudioClip, resp.AudioBytes);
			metrics.Success();
		}
	}
}
