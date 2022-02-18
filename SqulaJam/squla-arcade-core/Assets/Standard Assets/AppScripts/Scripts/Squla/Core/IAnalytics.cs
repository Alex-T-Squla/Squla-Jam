using System.Collections.Generic;

namespace Squla.Core
{
	public interface IAnalytics
	{
		void LogEvent (IDictionary<string, string> properties);

		void SetUserInfo (string userId, string userType);

		void UploadMetrics();
	}
}
