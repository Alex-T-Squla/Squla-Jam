using System.Globalization;
using Squla.Core.Audio;
using Squla.Core.i18n;
using Squla.Core.IOC;
using Squla.Core.Metrics;
using Squla.Core.Network;
using Squla.TDD;
using UnityEngine;

namespace Squla.Core.Tests
{
	[SingletonModule]
	public class Choosable_NetworkModule : MonoBehaviourV2, IAudioclipRepository, ILocaleConfiguration
	{
		public ICoroutineManager_Impl coroutineManager;

		protected override void AfterAwake ()
		{
			graph.RegisterModule (this);
		}

		[Provides]
		[Singleton]
		public IAudioclipRepository provideRepository ()
		{
			return this;
		}

		[Provides]
		[Singleton]
		public ICoroutineManager provideICoroutineManager_Impl ()
		{
			return coroutineManager;
		}

		[Provides]
		[Singleton]
		public ILocaleConfiguration provideILocaeConfiguration ()
		{
			return this;
		}

		[Provides]
		[Singleton]
		public IScreenHUD_UI provideScreenHUD (ScreenHUDMock uiMock)
		{
			return uiMock;
		}

		[Provides (Name = "Asset")]
		[Singleton]
		public INetworkManager provideAssetNetworkManager (IRequestExecutor_WebRequest androidExecutor, IMetricManager metricManager)
		{
			var metric = metricManager.CreateDeltaMetric(MetricNames.AssetQueue);
			return new INetworkManager_Impl (coroutineManager, androidExecutor, 5, metric);
		}

		[Provides (Name = "JSON")]
		[Singleton]
		public INetworkManager provideJSONNetworkManager (IRequestExecutor_WebRequest androidExecutor, IMetricManager metricManager)
		{
			var metric = metricManager.CreateDeltaMetric(MetricNames.JsonQueue);
			return new INetworkManager_Impl (coroutineManager, androidExecutor, 5, metric);
		}

		void IAudioclipRepository.Register (AudioClipDataSource dataSource)
		{
		}

		void IAudioclipRepository.Register (string clipName, AudioClip clip)
		{
		}

		public void Register(AudioSpriteDataSource spriteSource)
		{
		}

		void IAudioclipRepository.UnRegister (AudioClipDataSource dataSource)
		{
		}

		void IAudioclipRepository.UnRegister (string clipName)
		{
		}

		public void UnRegister(AudioSpriteDataSource spriteSource)
		{
		}

		AudioSource IAudioclipRepository.GetAudioSource ()
		{
			return null;
		}

		private readonly CultureInfo defaultCulture = new CultureInfo("nl-NL");

		CultureInfo ILocaleConfiguration.CurrentCulture => defaultCulture;

		string ILocaleConfiguration.URL_LOGIN => string.Empty;

		string ILocaleConfiguration.URL_API => string.Empty;

		string ILocaleConfiguration.URL_WSS => string.Empty;

		string ILocaleConfiguration.URL_EXCEPTION => string.Empty;

		string ILocaleConfiguration.CLIENT_ID => string.Empty;

		string ILocaleConfiguration.CLIENT_SECRET => string.Empty;

		bool ILocaleConfiguration.TimeoutEnabled => false;

		string ILocaleConfiguration.NETWORK_CHECK_URL => string.Empty;
	}
}
