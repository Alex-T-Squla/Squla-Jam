using Squla.Core.i18n;
using Squla.Core.IOC;
using Squla.Core.ZeroQ;

namespace Squla.Core.Logging
{
	public class LogMetaData : ILogMeta
	{
		private string userId;
		private SqulaLocale locale;

		private string context;

		[Inject]
		public LogMetaData (Bus bus)
		{
			bus.Register (this);
		}

		[Subscribe ("model://current/locale")]
		public void WhenLocaleChanged (SqulaLocale locale)
		{
			this.locale = locale;
//			context = string.Format("{0} | {1}", locale.Name, userId);
			context = locale.Name;
		}

		public string GetContext ()
		{
			return context;
		}

		public void SetUserId (string userId)
		{
			this.userId = userId;
//			context = string.Format("{0} | {1}", locale.Name, userId);
		}
	}
}
