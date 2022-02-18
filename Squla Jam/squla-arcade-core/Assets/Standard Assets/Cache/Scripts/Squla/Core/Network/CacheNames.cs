namespace Squla.Core.Network
{
	public sealed class CacheNames
	{
		public const string Navigation = "ContentNavigation";
		public const string Avatar = "AvatarCache";
		public const string Default = "Default";
		public const string Achievement = "ImageCacheAchievement";
		public const string Boss = "Boss";

		// *IMPORTANT* If you add a new cache name, remember to add it to this list
		// The first item will be automatically set initially
		public static readonly string[] CacheNamesList = {
			Default,
			Navigation,
			Avatar,
			Achievement
		};
	}
}