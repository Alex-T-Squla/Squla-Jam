using UnityEngine;
using System.Collections;

namespace Squla.Core.i18n
{
	
	public class SqulaLocale
	{
		public static readonly SqulaLocale Unknown = new SqulaLocale (string.Empty, -1);
		public static readonly SqulaLocale nl_NL = new SqulaLocale ("nl-NL", 1);
		public static readonly SqulaLocale en_US = new SqulaLocale ("en-US", 0);//Deprecated, but the item is used for fixing deprecation
		public static readonly SqulaLocale pl_PL = new SqulaLocale ("pl-PL", 2);
		public static readonly SqulaLocale zh_Hans_CN = new SqulaLocale ("zh-Hans-CN", 3);//Deprecated, but the item is used for fixing deprecation
		public static readonly SqulaLocale fr_FR = new SqulaLocale ("fr-FR", 4);
		public static readonly SqulaLocale de_DE = new SqulaLocale ("de-DE", 5);
		public static readonly SqulaLocale en_GB = new SqulaLocale ("en-GB", 6);//Deprecated, but the item is used for fixing deprecation
		public static readonly SqulaLocale es_ES = new SqulaLocale ("es-ES", 7);
		public static readonly SqulaLocale it_IT = new SqulaLocale ("it-IT", 8);//Deprecated, but the item is used for fixing deprecation

		public string Name {
			get;
			private set;
		}

		public int Id {
			get;
			private set;
		}

		private SqulaLocale (string locale, int id)
		{
			Name = locale;
			Id = id;
		}

		public override string ToString ()
		{
			return string.Format ("[Locale: locale={0}]", Name);
		}

		public static SqulaLocale FromId (int localeId)
		{
			if (localeId == nl_NL.Id)
				return nl_NL;
			
			if (localeId == en_US.Id) //NOTE; en_US is depricated but used for fixing deprecation
				return en_US;
			
			if (localeId == pl_PL.Id)
				return pl_PL;
			
			if (localeId == zh_Hans_CN.Id)//Deprecated, but the item is used for fixing deprecation
				return zh_Hans_CN;
			
			if (localeId == fr_FR.Id)
				return fr_FR;

			if (localeId == de_DE.Id)
				return de_DE;

			if (localeId == en_GB.Id) //NOTE; en_GB is depricated but used for fixing deprecation
				return en_GB;

			if (localeId == es_ES.Id)
				return es_ES;

			if (localeId == it_IT.Id)//Deprecated, but the item is used for fixing deprecation
				return it_IT;
			
			return Unknown;
		}

		public static SqulaLocale FromName (string name)
		{
			if (name == nl_NL.Name)
				return nl_NL;

			if (name == en_US.Name) //NOTE; en_US is depricated but used for fixing deprecation
				return en_US;

			if (name == pl_PL.Name)
				return pl_PL;

			if (name == zh_Hans_CN.Name)//Deprecated, but the item is used for fixing deprecation
				return zh_Hans_CN;

			if (name == fr_FR.Name)
				return fr_FR;

			if (name == de_DE.Name)
				return de_DE;

			if (name == en_GB.Name) //NOTE; en_GB is depricated but used for fixing deprecation
				return en_GB;

			if (name == es_ES.Name)
				return es_ES;

			if (name == it_IT.Name)//Deprecated, but the item is used for fixing deprecation
				return it_IT;

			return Unknown;
		}
	}

}
