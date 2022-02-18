using UnityEngine;
using Squla.Core.IOC;
using Squla.Core.Audio;
using Squla.Core.ZeroQ;

namespace Squla.Core.Network
{
	[SingletonModule]
	public class TDD_DownloaderModule : MonoBehaviourV2
	{
		private IImageDownloader imageDownloader;
		private IAudioDownloader audioDownloader;
		private ITextDownloader textDownloader;

		protected override void AfterAwake ()
		{
			graph.RegisterModule (this);

			var coroutineImpl = graph.Get<ICoroutineManager>();
			var internetImageDownloader = graph.Get<InternetImageDownloader>();
			var audioRepo = graph.Get<IAudioclipRepository>();
			var internetAudioDownloader = graph.Get<InternetAudioDownloader>();
			var networkManager = graph.Get<INetworkManager>("Asset");

			var l2Image = new L2ImageDownloader (100, Application.persistentDataPath + "/Tester/Image", coroutineImpl, internetImageDownloader, bus);
			imageDownloader = new L1ImageDownloader (100, l2Image);

			var l2Audio = new L2AudioDownloader (100, Application.persistentDataPath + "/Tester/Audio", coroutineImpl, internetAudioDownloader, bus);
			audioDownloader = new L1AudioDownloader (100, l2Audio, audioRepo);

			textDownloader = new L2TextDownloader(10, Application.persistentDataPath + "/Tester/Data", coroutineImpl, networkManager, bus);
		}

		[Singleton]
		[Provides (Name = CacheNames.Avatar)]
		public IImageDownloader provideImageCacheAvatar ()
		{
			return imageDownloader;
		}

		[Singleton]
		[Provides (Name = CacheNames.Navigation)]
		public IImageDownloader provideContentNavigationImage ()
		{
			return imageDownloader;
		}

		[Singleton]
		[Provides(Name = CacheNames.Navigation)]
		public ITextDownloader provideContentNavigationText(ICoroutineManager manager, [Inject("Asset")]INetworkManager networkManager)
		{
			return textDownloader;
		}

		[Singleton]
		[Provides (Name = CacheNames.Default)]
		public IImageDownloader provideImageCache20 ()
		{
			return imageDownloader;
		}

		[Singleton]
		[Provides (Name = CacheNames.Achievement)]
		public IImageDownloader provideAchievementImage ()
		{
			return imageDownloader;
		}

		[Singleton]
		[Provides (Name = "ImageCache10")]
		public IImageDownloader provideImageCache10 ()
		{
			return imageDownloader;
		}

		[Singleton]
		[Provides (Name = CacheNames.Navigation)]
		public IAudioDownloader provideContentNavigationAudio ()
		{
			return audioDownloader;
		}

		[Singleton]
		[Provides (Name = CacheNames.Default)]
		public IAudioDownloader provideAudioCache20 ()
		{
			return audioDownloader;
		}

		[Singleton]
		[Provides (Name = "ComponentTester")]
		public IImageDownloader provideComponentTesterImages ()
		{
			return imageDownloader;
		}

		[Singleton]
		[Provides (Name = "ComponentTester")]
		public IAudioDownloader provideComponentTesterAudio ()
		{
			return audioDownloader;
		}

		[Singleton]
		[Provides(Name = CacheNames.Boss)]
		public IAudioDownloader provideBossAudio()
		{
			return audioDownloader;
		}

		[Singleton]
		[Provides(Name = CacheNames.Boss)]
		public IImageDownloader provideBossSprite()
		{
			return imageDownloader;
		}

		[Singleton]
		[Provides(Name = CacheNames.Boss)]
		public ITextDownloader provideBossData(ICoroutineManager manager, [Inject("Asset")]INetworkManager networkManager)
		{
			return textDownloader;
		}

	}
}
