using UnityEngine;

namespace Squla.Core
{
	[CreateAssetMenu(menuName = "Squla/URLPaths")]
	public class SqulaURLConfiguration : ScriptableObject
	{
		[Header("App URLs")] public string logAppEvent;
		public string metrics;
		public string exceptions;
		public string pushRegister;
		public string appReview;
		public string logging;

		[Header("Parent URLs")] public string parentHome;
		public string parentCheckTransaction;
		public string parentChildRegister;
		public string parentChildRenew;
		public string parentRegister;
		public string parentRegisterEmailName;

		[Header("Student URLs")] public string loginMe;
		public string init;

		[Header("Home URLs")] public string home;
		public string weekGoal;
		public string playerData;

		public string messages;
		public string activity;

		[Header("Tabs URLs")] public string friends;
		public string shop;
		public string profile;

		public string liveBattleRooms;

		public string recoverPassword;
		public string recoverUsername;

		[Header("Shop")] public string shopUrlAndroid;
		public string shopUrl2Android;
		public string shopUrliOS;
		public string shopUrl2iOS;

		[Header("Others")] public string wssNode;
		public string testManager;

		// Platform Dependent parameters
#if UNITY_IOS
		public string SHOP_URL_ENDPOINT => shopUrliOS;
		public string SHOP_URL_ENDPOINT_TWO => shopUrl2iOS;
#elif UNITY_ANDROID
		public string SHOP_URL_ENDPOINT => shopUrlAndroid;
		public string SHOP_URL_ENDPOINT_TWO => shopUrl2Android;
#else
		public string SHOP_URL_ENDPOINT => "";
		public string SHOP_URL_ENDPOINT_TWO => "";
#endif
	}
}