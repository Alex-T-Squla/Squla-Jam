using System.Collections;
using Squla.Core.IOC;
using Squla.Core.Logging;
using UnityEngine;

namespace Squla.Core.Network
{
	/// <summary>
	/// No [Sigleton] for this class.
	/// </summary>
	public class IRequestExecutor_iOS: IRequestExecuter
	{
		/// <summary>
		/// Maximum seconds task can take to download an asset before declaring it as errored.
		/// </summary>
		private const float MAX_SECONDS_PER_TRY_WHILE_NOT_REACHABLE = 2f;

		private const float MAX_SECONDS_PER_TRY_WHILE_REACHABLE = 5f;

		private const float SLEEP_SECONDS_IN_WHILE_LOOP = 0.2f;
		
		private readonly SmartLogger logger;

		private readonly IRequestExecutor_Android androidExecutor;
		
		private readonly ICoroutineManager manager;
		
		[Inject]
		public IRequestExecutor_iOS (ICoroutineManager manager, IRequestExecutor_Android androidExecutor)
		{
			logger = SmartLogger.GetLogger<IRequestExecutor_iOS> ();
			this.manager = manager;
			this.androidExecutor = androidExecutor;
		}

		public void Execute (DataRequest request)
		{
			manager.CreateCoroutine(Master(request));
		}
		
		private IEnumerator Master (DataRequest request)
		{
			var workerCoroutine = manager.CreateCoroutine(androidExecutor.Worker(request, 1));
			var notReachableLoopIndex = 0;
			var reachableLoopIndex = 0;
			const int notReachableLoopMax = (int)(MAX_SECONDS_PER_TRY_WHILE_NOT_REACHABLE / SLEEP_SECONDS_IN_WHILE_LOOP);
			const int reachableLoopMax = (int)(MAX_SECONDS_PER_TRY_WHILE_REACHABLE / SLEEP_SECONDS_IN_WHILE_LOOP);

			var needsKill = false;
			
			while (request.Status == RequestStatus.Downloading) {
				yield return new WaitForSeconds (SLEEP_SECONDS_IN_WHILE_LOOP);

				bool isNetworkReachable = Application.internetReachability != NetworkReachability.NotReachable;
				if (isNetworkReachable) {
					// iOS in reachable network situation, if no route to host also hangs. so wait for max allowed time and quite.
					reachableLoopIndex++;

					if (reachableLoopIndex == reachableLoopMax) {
						needsKill = true;
						logger.Debug ("reachable kill {0}", request.url);
					}
				} else {
					// check for network reachability.
					// if max amount of time reached (iOS) for the current try then it is considered errored. so you can retry.
					notReachableLoopIndex++;

					if (notReachableLoopIndex == notReachableLoopMax) {
						needsKill = true;
						logger.Debug ("non reachable kill {0}", request.url);
					}
				}

				if (!needsKill) 
					continue;
				
				request.Abort();
				manager.StopCoroutine(workerCoroutine);
				if (request.onComplete == null) {
					logger.Debug("DataRequest.onComplete is null for {0}", request.url);
				} else {
					var response = new IDataResponse_Null (0);
					request.onComplete (request, response);
				}
				break;
			}
		}
	}
}

