using System;
using System.Collections.Generic;
using UnityEngine;
using Squla.Core.IOC;
using Squla.Core.Metrics;

namespace Squla.Core.Network
{
	[Singleton]
	public class SpriteRequestManager
	{
		private readonly INetworkManager manager;

		private readonly RequestMetrics metrics;

		private readonly List<DataRequest> inProgressRequests;

		[Inject]
		public SpriteRequestManager ([Inject ("Asset")] INetworkManager networkManager, IMetricManager metricManager)
		{
			manager = networkManager;
			metrics = new RequestMetrics ("sprite", metricManager);
			inProgressRequests = new List<DataRequest> ();
		}

		public void GET (string spriteURL, Action<string, int, Sprite> onComplete)
		{
			metrics.Request ();

			var req = new DataRequest {
				requestType = DataRequest.RequestType.Image,
				url = spriteURL,
				source = onComplete,
				onComplete = Response
			};

			inProgressRequests.Add (req);
			manager.Execute (req);
		}

		private void Response (DataRequest req, IDataResponse resp)
		{
			inProgressRequests.Remove (req);
			metrics.Response (resp.ResponseCode);

			var onComplete = (Action<string, int, Sprite>)req.source;
			var sprite = resp.Sprite;

			if (sprite != null) {
				sprite.name = req.url;
				sprite.texture.name = req.url;
			}

			onComplete (req.url, resp.ResponseCode, sprite);
			metrics.Success();
		}

		public void Flush ()
		{
			for(var i=0; i<inProgressRequests.Count; i++)
				inProgressRequests[i].Abort ();

			inProgressRequests.Clear ();
		}
	}
}
