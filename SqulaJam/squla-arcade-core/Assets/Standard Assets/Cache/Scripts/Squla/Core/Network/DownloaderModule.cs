using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Squla.Core.IOC;
using Squla.Core.Audio;
using Squla.Core.ZeroQ;

namespace Squla.Core.Network
{
	/// <summary>
	/// Whatever [Provides] you add here corresponding change needs to be done in TDD_DownloaderModule.cs
	/// </summary>
	[SingletonModule]
	public class DownloaderModule : MonoBehaviourV2
	{
		private WaitForSeconds sleep;

		private List<L1ImageDownloader> l1ImageDownLoaders;

		protected override void AfterAwake ()
		{
			graph.RegisterModule (this);
			sleep = new WaitForSeconds(30.0f);
			l1ImageDownLoaders = new List<L1ImageDownloader> ();

			StartCoroutine (Cleanup ());
		}

		private IEnumerator Cleanup()
		{
			while (true) {
				yield return sleep;
				logger.Debug ("------Asset cleanup");
				for(var i=0; i<l1ImageDownLoaders.Count; i++)
					l1ImageDownLoaders[i].FlushSecondary ();

				yield return Resources.UnloadUnusedAssets ();
			}
		}

		[Singleton]
		[Provides (Name = CacheNames.Avatar)]
		public IImageDownloader provideImageCacheAvatar (InternetImageDownloader internet, ICoroutineManager impl)
		{
			var l2 = new L2ImageDownloader (150, Application.persistentDataPath + "/AvatarCache", impl, internet, bus);
			var l1 = new L1ImageDownloader (20, l2);
			l1ImageDownLoaders.Add (l1);
			return l1;
		}

		[Singleton]
		[Provides (Name = CacheNames.Navigation)]
		public IImageDownloader provideContentNavigationImage (InternetImageDownloader internet, ICoroutineManager impl)
		{
			var l2 = new L2ImageDownloader (200, Application.persistentDataPath + "/NavigationImageCache", impl, internet, bus);
			var l1 = new L1ImageDownloader (20, l2);
			l1ImageDownLoaders.Add (l1);
			return l1;
		}

		[Singleton]
		[Provides(Name = CacheNames.Navigation)]
		public ITextDownloader provideContentNavigationText(ICoroutineManager manager, [Inject("Asset")]INetworkManager networkManager)
		{
			var l2 = new L2TextDownloader(10, Application.persistentDataPath + "/NavigationTextCache", manager, networkManager, bus);
			return l2;
		}

		[Singleton]
		[Provides (Name = CacheNames.Default)]
		public IImageDownloader provideImageCache20 (InternetImageDownloader internet)
		{
			var l1 = new L1ImageDownloader (40, internet);
			l1ImageDownLoaders.Add (l1);
			return l1;
		}

		[Singleton]
		[Provides (Name = CacheNames.Achievement)]
		public IImageDownloader provideAchievementImage (InternetImageDownloader internet, ICoroutineManager impl)
		{
			var l2 = new L2ImageDownloader (200, Application.persistentDataPath + "/Achievement", impl, internet, bus);
			var l1 = new L1ImageDownloader (10, l2);
			l1ImageDownLoaders.Add (l1);
			return l1;
		}

		[Singleton]
		[Provides (Name = "ImageCache10")]
		public IImageDownloader provideImageCache10 (InternetImageDownloader internet)
		{
			var l1 = new L1ImageDownloader (10, internet);
			l1ImageDownLoaders.Add (l1);
			return l1;
		}

		[Singleton]
		[Provides (Name = CacheNames.Navigation)]
		public IAudioDownloader provideContentNavigationAudio (IAudioclipRepository repository, InternetAudioDownloader internet, ICoroutineManager impl)
		{
			var l2 = new L2AudioDownloader (100, Application.persistentDataPath + "/NavigationAudioCache", impl, internet, bus);
			return new L1AudioDownloader (20, l2, repository);
		}

		[Singleton]
		[Provides (Name = CacheNames.Default)]
		public IAudioDownloader provideAudioCache20 (IAudioclipRepository repository, InternetAudioDownloader internet)
		{
			return new L1AudioDownloader (40, internet, repository);
		}

		[Singleton]
		[Provides(Name = CacheNames.Boss)]
		public IAudioDownloader provideBossAudio(IAudioclipRepository repository, InternetAudioDownloader internet,
			ICoroutineManager impl)
		{
			var l2 = new L2AudioDownloader (18, Application.persistentDataPath + "/Boss/Audio", impl, internet, bus);
			return new L1AudioDownloader (12, l2, repository);
		}

		[Singleton]
		[Provides(Name = CacheNames.Boss)]
		public IImageDownloader provideBossSprite(InternetImageDownloader internet, ICoroutineManager impl)
		{
			var l2 = new L2ImageDownloader (5, Application.persistentDataPath + "/Boss/Sprite", impl, internet, bus);
			var l1 = new L1ImageDownloader (3, l2);
			l1ImageDownLoaders.Add (l1);
			return l1;
		}

		[Singleton]
		[Provides(Name = CacheNames.Boss)]
		public ITextDownloader provideBossData(ICoroutineManager manager, [Inject("Asset")]INetworkManager networkManager)
		{
			var l2 = new L2TextDownloader(13, Application.persistentDataPath + "/Boss/Data", manager, networkManager, bus);
			return l2;
		}

	}
}
