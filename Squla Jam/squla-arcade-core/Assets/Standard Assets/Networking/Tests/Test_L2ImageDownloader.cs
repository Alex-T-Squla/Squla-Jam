using Squla.Core;
using Squla.Core.IOC;
using Squla.Core.Network;
using UnityEngine;

namespace Squla.Standard_Assets.Networking.Example
{
	public class Test_L2ImageDownloader : MonoBehaviourV2
	{

		[Inject]
		private ICoroutineManager courImpl;

		[Inject]
		private InternetImageDownloader fallback;

		public StringListDataSet urlDataSet;
		private string[] urls;

		private L2ImageDownloader downloader;

		void Start ()
		{
			urls = urlDataSet.data;
			downloader = new L2ImageDownloader (100, Application.persistentDataPath + "/Tester", courImpl, fallback, bus);
			foreach (var url in urls) {
				downloader.GetImage (url, OnCompleted);
			}
		}

		void OnCompleted (string url, Sprite sprite)
		{
			logger.Debug ("cache hit {0} {1}", url, sprite != null);
		}
	}
}
