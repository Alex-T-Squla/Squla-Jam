using System;
using System.Collections.Generic;
using Squla.Core.Logging;
using Squla.Core.Metrics;

namespace Squla.Core.Network
{
	internal class INetworkManager_Impl : INetworkManager
	{
		private readonly SmartLogger logger;

		private readonly Queue<DataRequest> waitingQueue = new Queue<DataRequest> ();

		private readonly ICoroutineManager coroutineManager;

		private readonly IRequestExecuter driver;

		private readonly int maxSize;

		private readonly DeltaMetric inProgressMetric;

		public INetworkManager_Impl (ICoroutineManager coroutineManager, IRequestExecuter driver, int maxSize, DeltaMetric inProgressMetric)
		{
			logger = SmartLogger.GetLogger<INetworkManager_Impl> ();
			this.coroutineManager = coroutineManager;
			this.driver = driver;
			this.maxSize = maxSize;
			this.inProgressMetric = inProgressMetric;
		}

		public void Execute (DataRequest request)
		{
			if (string.IsNullOrEmpty (request.url)) {
				logger.Debug ("bypass Execute due to null url");
				var resp = new IDataResponse_Null (200);
				request.Status = RequestStatus.Ready;
				request.onComplete (request, resp);
				return;
			}

			if (inProgressMetric.Count >= maxSize) {
				request.Status = RequestStatus.Queued;
				waitingQueue.Enqueue (request);
				logger.Debug ("Queued: {0} {1} {2}", waitingQueue.Count, request.Method, request.url);
				return;
			}

			inProgressMetric.Increment();
			logger.Debug ("Execute: {0} {1}", request.Method, request.url);

			var originalOnComplete = request.onComplete;
			request.onComplete = (req, resp) => {
				OnComplete (req, resp, originalOnComplete);
			};

			driver.Execute (request);
		}

		private void OnComplete (DataRequest request, IDataResponse response, Action<DataRequest, IDataResponse> originalOnComplete)
		{
			inProgressMetric.Decrement();
			coroutineManager.ExecuteInNextFrame(DequeIfPossible);

			// set onComplete to null, so that next time it won't be called even by accident.
			request.onComplete = null;

			logger.Debug ("OnComplete: {0} {1} {2}", request.Method, request.url, response.ResponseCode);
			originalOnComplete (request, response);

			// the below code may produce infinite spinner. that's why moved up.
//			inProgressMetric.Decrement();
//			coroutineManager.ExecuteInNextFrame(DequeIfPossible);
		}

		private void DequeIfPossible()
		{
			if (waitingQueue.Count > 0) {
				var newReq = waitingQueue.Dequeue ();
				Execute (newReq);
			}
		}
	}
}
