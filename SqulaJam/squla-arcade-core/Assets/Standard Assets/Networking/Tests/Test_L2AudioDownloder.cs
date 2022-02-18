using Squla.Core;
using Squla.Core.IOC;
using Squla.Core.Network;
using Squla.Core.ZeroQ;
using UnityEngine;

namespace Squla.Standard_Assets.Networking.Example
{
	public class Test_L2AudioDownloder : MonoBehaviourV2
	{

		[Inject]
		private ICoroutineManager courImpl;

		[Inject]
		private InternetAudioDownloader fallback;

		public StringListDataSet urlDataSet;
		private string[] urls;

		private L2AudioDownloader downloader;

		void Start ()
		{
			urls = urlDataSet.data;
			downloader = new L2AudioDownloader (100, Application.persistentDataPath + "/Tester", courImpl, fallback, bus);
			foreach (var url in urls) {
				downloader.GetAudioClip(url, OnCompleted);
			}
		}

		void OnCompleted (string url, AudioClip clip, byte[] bytes)
		{
			logger.Debug ("cache hit {0} {1}", url, clip != null);
		}
	}
}
