using Squla.Core;
using Squla.Core.IOC;
using Squla.Core.Network;
using Squla.Core.TDD;

namespace Squla.Standard_Assets.Networking.Example
{
	public class Test_L1ImageDownloader : MonoBehaviourV2
	{
		[Inject] private ITestManager testManager;

		[Inject]
		private ICoroutineManager courImpl;

		[Inject]
		private InternetImageDownloader fallback;

		public StringListDataSet urlDataSet;
		private string[] urls;

		private L1ImageDownloader downloader;

		protected override void AfterAwake()
		{
			testManager.CreateTestCase("Default", TestCase1, timeout:30);
		}

		private void TestCase1 ()
		{
			urls = urlDataSet.data;
			downloader = new L1ImageDownloader (urls.Length, fallback);
			downloader.Download (new Batch (urls, OnCompleted));
		}

		private void OnCompleted ()
		{
			foreach(var url in urls) {
				var sprite = downloader.GetImage (url);
				testManager.Assert(sprite != null, "sprite {0} not found in the cache", url);
				logger.Debug ("cache hit {0} {1}", url, sprite != null);
			}
			testManager.TestCaseEnded();
		}
	}
}
