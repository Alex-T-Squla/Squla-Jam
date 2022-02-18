
namespace Squla.Core.i18n
{
	public interface ILocaleManager
	{
		SqulaLocale Locale { get; }

		bool IsFirstTime { get; }

		void ChangeLocale (SqulaLocale locale);
	}
}
