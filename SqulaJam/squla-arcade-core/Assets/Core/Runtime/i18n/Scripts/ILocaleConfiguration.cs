using System.Globalization;

namespace Squla.Core.i18n
{
	public interface ILocaleConfiguration
	{
		CultureInfo CurrentCulture { get; }

		bool TimeoutEnabled { get; }

		string URL_LOGIN { get; }

		string URL_API { get; }

		string URL_WSS { get; }

		string URL_EXCEPTION { get; }

		string CLIENT_ID { get; }

		string CLIENT_SECRET { get; }

		string NETWORK_CHECK_URL { get; }

		string ComputeHash(params object[] args);
	}
}
