using UnityEngine;
using Squla.Core.IOC;
using Squla.Core.Network;
using Squla.Core.TDD;

namespace Squla.Core.Tests
{
	public class Test_AudioRequstManger : MonoBehaviourV2
	{
		[InjectComponent("metricView")] private RequestMetricView view;

		[Inject] private AudioRequestManager manager;

		[Inject] private ITestManager testManager;

		private int respCount;

		public StringListDataSet urlDataSet;
		private string[] urls;

		protected override void AfterAwake ()
		{
			urls = urlDataSet.data;
			view.Init("audio");

			testManager.CreateTestCase("Default", TestCase1, timeout:30);
		}

		private void TestCase1 ()
		{
			foreach(var url in urls) {
				manager.GET (url, onResponse);
			}
		}

		void onResponse (string url, AudioClip audio, byte[] bytes)
		{
			respCount++;
			logger.Debug ("response {0} {1}", respCount, url);

			testManager.Assert(respCount<=urls.Length, "completed requests {0}", respCount);

			if (respCount >= urls.Length)
				testManager.TestCaseEnded();
		}
	}
}
