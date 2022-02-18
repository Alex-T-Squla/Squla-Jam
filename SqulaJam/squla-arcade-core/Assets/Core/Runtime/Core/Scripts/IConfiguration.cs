namespace Squla.Core
{
	public interface IConfiguration
	{
		string Version { get; }

		string BranchName { get; }

		int BuildNumber { get; }

		bool IsStaging { get; }

		bool SkipPauseBehaviour { get; }

		bool SkipUpdateScreen { get; }

		string DeviceOS { get; }

		string DeviceType { get; }

		string DeviceId { get; }

		string DeviceId_b64 { get; }

		string UserAgent { get; }

		string WS_TestServerUrl { get; }

		string HTTP_TestServerUrl { get; }

		string ClientStoreUrl { get; }

		bool RemoteLoggingEnabled { get; }
		
		bool WebrequestProgressRetry { get; set; }
	}
}

