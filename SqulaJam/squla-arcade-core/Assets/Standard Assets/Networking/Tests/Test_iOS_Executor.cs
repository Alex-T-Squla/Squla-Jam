using UnityEngine;
using Squla.Core.IOC;
using Squla.Core.Metrics;
using Squla.Core.Network;
using Squla.Core.TDD;

namespace Squla.Core.Tests
{
	public class Test_iOS_Executor : MonoBehaviourV2
	{
		[InjectComponent("metricView")] private RequestMetricView view;

		[Inject] private ITestManager testManager;

		[Inject] private ICoroutineManager courImpl;

		[Inject] private IRequestExecutor_iOS executor;

		[Inject] private IConfiguration config;

		[Inject] private IMetricManager metricManager;

		private RequestMetrics metrics;

		private INetworkManager manager;

		private int respCount;
		private const int REQUESTS_COUNT = 10;

		protected override void AfterAwake ()
		{
			view.Init("json");
			metrics = new RequestMetrics ("json", metricManager);

			testManager.CreateTestCase("Default", TestCase1, timeout:30);
		}

		private void TestCase1 ()
		{
			var metric = metricManager.CreateDeltaMetric(MetricNames.JsonQueue);
			manager = new INetworkManager_Impl(courImpl, executor, 5, metric);

			for (var i = 0; i < 50; i++) {
				var req = new DataRequest {
					url = string.Format("{0}/sleep/{1}?id={2}", config.HTTP_TestServerUrl , Random.Range (1, 10), i),
					onComplete = Response
				};

				metrics.Request();
				manager.Execute (req);
			}
		}

		private void Response (DataRequest req, IDataResponse resp)
		{
			metrics.Response(resp.ResponseCode);
			respCount++;
			logger.Debug ("response {0} {1}", respCount, req.url);
			testManager.Assert(respCount<=REQUESTS_COUNT, "completed requests {0}", respCount);

			if (respCount >= REQUESTS_COUNT)
				testManager.TestCaseEnded();

			metrics.Success();
		}
	}
}
