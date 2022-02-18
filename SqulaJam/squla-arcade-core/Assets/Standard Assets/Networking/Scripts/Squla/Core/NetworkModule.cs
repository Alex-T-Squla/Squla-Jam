using Squla.Core.IOC;
using Squla.Core.Metrics;
using UnityEngine;

namespace Squla.Core.Network
{
	[SingletonModule]
	public class NetworkModule : MonoBehaviourV2
	{
		public ICoroutineManager_Impl coroutineManager;
		public IRequestExecutor_NativeAndroid nativeAndroid;

		[Inject]
		private IConfiguration config;

		private string MetricsPath => Application.persistentDataPath + "/Metrics";

		[Provides]
		[Singleton]
		public ICoroutineManager provideICoroutineManager_Impl ()
		{
			return coroutineManager;
		}

		[Provides (Name = "JSON")]
		[Singleton]
		public INetworkManager provideJSONNetworkManager (IRequestExecutor_WebRequest executor, IMetricManager metricManager)
		{
			IRequestExecuter target = executor;
//			#if UNITY_EDITOR
//				// do nothing
//			#elif UNITY_ANDROID
//				if (getSDKInt() >= 21) {  // LOLLIPOP
//					nativeAndroid.gameObject.SetActive (true);
//					target = nativeAndroid;
//				}
//			#endif

			var metric = metricManager.CreateDeltaMetric(MetricNames.JsonQueue);
			return new INetworkManager_Impl (coroutineManager, target, 5, metric);
		}

		static int getSDKInt() {
			using (var version = new AndroidJavaClass("android.os.Build$VERSION")) {
				return version.GetStatic<int>("SDK_INT");
			}
		}


		[Provides (Name = "Asset")]
		[Singleton]
		public INetworkManager provideAssetNetworkManager (IRequestExecutor_WebRequest executor, IMetricManager metricManager)
		{
			IRequestExecuter target = executor;
			// #if UNITY_EDITOR
			// // do nothing
			// #elif UNITY_ANDROID
			// 	nativeAndroid.gameObject.SetActive (true);
			// 	target = nativeAndroid;
			// #endif

			var metric = metricManager.CreateDeltaMetric(MetricNames.AssetQueue);
			return new INetworkManager_Impl (coroutineManager, target, 5, metric);
		}

		[Provides]
		[Singleton]
		public IAuthService provideAuthService (AuthService service)
		{
			return service;
		}

		[Provides (Name = "SafeBasicAuth")]
		[Singleton]
		public IApiService provideBasicApiService (BasicApiService service)
		{
			service.responsePolicy = new SafePolicy ();
			return service;
		}

		[Provides (Name = "SafestBasicAuth")]
		[Singleton]
		public IApiService provideSafestBasicApiService (BasicApiService service)
		{
			service.responsePolicy = new SafestPolicy ();
			return service;
		}

		[Provides]
		[Singleton]
		public IApiService provideOAuth2ApiService (OAuth2ApiService service)
		{
			service.responsePolicy = new DefaultPolicy ();
			return service;
		}

		[Provides (Name = "SafestOAuth2")]
		[Singleton]
		public IApiService provideSafestOAuth2ApiService (OAuth2ApiService service)
		{
			service.responsePolicy = new SafestPolicy ();
			return service;
		}

		[Provides (Name = "SafeOAuth2")]
		[Singleton]
		public IApiService provideSafeOAuth2ApiService (OAuth2ApiService service)
		{
			service.responsePolicy = new SafePolicy ();
			return service;
		}

		[Provides]
		[Singleton]
		public IAnalytics provideAnalyticsService (AnalyticsService service, IMetricManager metricManager)
		{
			service.metricsPath = metricManager.MetricsDataPath;
			return service;
		}

		protected override void AfterAwake ()
		{
			graph.RegisterModule (this);
		}
	}

}
