using Squla.Core;
using Squla.Core.Audio;
using Squla.Core.IOC;
using Squla.Core.Network;

namespace Squla.Standard_Assets.Networking.Example
{
	public class Test_L1AudioDownloder : MonoBehaviourV2
	{
		[Inject]
		private ICoroutineManager courImpl;

		[Inject]
		private InternetAudioDownloader fallback;

		[Inject]
		private IAudioclipRepository audioRepo;

		public StringListDataSet urlDataSet;
		private string[] urls;

		private L1AudioDownloader downloader;

		void Start ()
		{
			urls = urlDataSet.data;
			downloader = new L1AudioDownloader (100, fallback, audioRepo);
			downloader.Download (new Batch (urls, OnCompleted));
		}

		void OnCompleted ()
		{
			foreach(var url in urls) {
				var clip = downloader.GetAudioClip (url);
				logger.Debug ("cache hit {0} {1}", url, clip != null);
			}
		}
	}
}
