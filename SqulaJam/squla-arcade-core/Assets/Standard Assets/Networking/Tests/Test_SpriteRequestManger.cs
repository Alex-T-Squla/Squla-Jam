using UnityEngine;
using Squla.Core.IOC;
using Squla.Core.Network;
using Squla.Core.TDD;

namespace Squla.Core.Tests
{
	public class Test_SpriteRequestManger : MonoBehaviourV2
	{
		[Inject] private ITestManager testManager;

		[InjectComponent("metricView")] private RequestMetricView view;

		[Inject] private SpriteRequestManager manager;

		public StringListDataSet urlDataSet;
		private string[] urls;
		private int respCount;

		protected override void AfterAwake ()
		{
			urls = urlDataSet.data;
			view.Init("sprite");

			testManager.CreateTestCase("Default", TestCase1, timeout:30);
		}

		private void TestCase1 ()
		{
			foreach(var url in urls) {
				manager.GET (url, onResponse);
			}
		}

		void onResponse (string url, int httpStatus, Sprite sprite)
		{
			respCount++;
			logger.Debug ("response {0} {1} {2}", respCount, url, httpStatus);
			if (respCount >= urls.Length)
				testManager.TestCaseEnded();
		}
	}
}
