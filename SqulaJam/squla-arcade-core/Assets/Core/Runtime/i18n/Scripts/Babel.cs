using UnityEngine;
using Squla.Core.IOC;
using System.Collections;

namespace Squla.Core.i18n
{
	[Singleton]
	public class Babel
	{
		private ITranslationProvider _translationProvider;

		private ILocaleManager localeManager;

		[Inject]
		public Babel (ILocaleManager localeManager, ITranslationProvider translationProvider)
		{
			this.localeManager = localeManager;
			this._translationProvider = translationProvider;
		}

		public string GetText (string key)
		{
			LanguageAsset translations = _translationProvider.GetTranslations (localeManager.Locale);
			if (translations == null) {
				return key;
			}
			return translations.GetTranslation (key);
		}

		public string GetText (string key, System.Object arg0)
		{
			var formatString = GetText (key);
			return string.Format (formatString, arg0);
		}

		public string GetText (string key, System.Object[] args)
		{
			var formatString = GetText (key);
			return string.Format (formatString, args);
		}
	}
}
