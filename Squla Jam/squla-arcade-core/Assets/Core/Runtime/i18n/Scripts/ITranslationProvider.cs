namespace Squla.Core.i18n
{
	public interface ITranslationProvider
	{
		LanguageAsset GetTranslations (SqulaLocale locale);
	}
}
