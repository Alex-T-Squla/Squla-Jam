using System.Collections.Generic;
using Squla.Core.IOC;
using Squla.Core.ZeroQ;
using UnityEngine;

namespace Squla.Core.i18n
{
    public class LocaleManager : ILocaleManager
    {
        private SqulaLocale _locale;

        private Bus _bus;

        private bool _firstTime;

        [Inject]
        public LocaleManager(Bus bus)
        {
            _bus = bus;
            _bus.Register(this);

            LoadLocale();
        }

        SqulaLocale ILocaleManager.Locale {
            get { return _locale; }
        }

        void ILocaleManager.ChangeLocale(SqulaLocale locale)
        {
            bool wasFirstTime = _firstTime;
            bool modified = _locale != locale || _firstTime;
            _locale = locale;
            _firstTime = false;
            PlayerPrefs.SetInt("locale", locale.Id);
            _bus.Publish("cmd://settings/locale/changed", modified);

            if (modified) {
                _bus.Publish("model://current/locale", _locale);
            }

            _bus.Publish("cmd://analytics/log-event", new Dictionary<string, string>() {
                { "event_name", "app-locale-changed" },
                { "locale", locale.Name },
                { "is_first_time", wasFirstTime.ToString() }
            });
        }

        bool ILocaleManager.IsFirstTime {
            get { return _firstTime; }
        }

        private void LoadLocale()
        {
            int language = PlayerPrefs.GetInt("language", SqulaLocale.Unknown.Id);
            int localeId = PlayerPrefs.GetInt("locale", SqulaLocale.Unknown.Id);

            var locale = SqulaLocale.FromId(localeId);

            if (language != SqulaLocale.Unknown.Id) {
                // langauge to Locale conversion
                locale = SqulaLocale.FromId(language);

                // and delete "language" preference.
                PlayerPrefs.DeleteKey("language");
            }

            if (isDeprecatedLanguage(locale.Id)) {
                // deprecatedlanguage conversion to nl_NL
                locale = SqulaLocale.nl_NL;
                PlayerPrefs.SetInt("locale", locale.Id);
            }

            if (locale.Id == SqulaLocale.Unknown.Id) {
                locale = GetSystemLocale();
                _firstTime = true;
            }

            _locale = locale;
            _bus.Publish("model://current/locale", _locale);
        }

        private bool isDeprecatedLanguage(int locale_id)
        {
            return
                locale_id == SqulaLocale.en_GB.Id ||
                locale_id == SqulaLocale.en_US.Id ||
                locale_id == SqulaLocale.it_IT.Id ||
                locale_id == SqulaLocale.zh_Hans_CN.Id
                ;
        }

        private SqulaLocale GetSystemLocale()
        {
            var lang = Application.systemLanguage;

            switch (lang) {
                case SystemLanguage.Polish:
                    return SqulaLocale.pl_PL;
                case SystemLanguage.Dutch:
                    return SqulaLocale.nl_NL;
                case SystemLanguage.German:
                    return SqulaLocale.de_DE;
                default:
                    return SqulaLocale.nl_NL;
            }
        }
    }
}