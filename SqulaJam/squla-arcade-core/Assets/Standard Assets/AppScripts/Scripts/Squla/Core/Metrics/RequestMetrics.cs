
namespace Squla.Core.Metrics
{
	public sealed class RequestMetrics
	{
		public readonly CountMetric requested;
		public readonly CountMetric completed;
		public readonly CountMetric http200;
		public readonly CountMetric success;
		public readonly CountMetric http404;
		public readonly CountMetric http400;
		public readonly CountMetric http500;
		public readonly CountMetric http0;
		public readonly CountMetric http5xx;
		public readonly DeltaMetric inProgress;
		public readonly DeltaMetric inProgressSuccess;

		public RequestMetrics (string name, IMetricManager metricManager)
		{
			requested = metricManager.CreateCountMetric (
				string.Format ("count://network-requests/{0}/requested", name));
			completed = metricManager.CreateCountMetric (
				string.Format ("count://network-requests/{0}/completed", name));
			http200 = metricManager.CreateCountMetric(
				string.Format ("count://network-requests/{0}/200", name));
			success = metricManager.CreateCountMetric(
				string.Format ("count://network-requests/{0}/success", name));
			http404 = metricManager.CreateCountMetric (
				string.Format ("count://network-requests/{0}/404", name));
			http400 = metricManager.CreateCountMetric (
				string.Format ("count://network-requests/{0}/400", name));
			http500 = metricManager.CreateCountMetric (
				string.Format ("count://network-requests/{0}/500", name));
			http5xx = metricManager.CreateCountMetric (
				string.Format ("count://network-requests/{0}/5xx", name));
			http0 = metricManager.CreateCountMetric (
				string.Format ("count://network-requests/{0}/0", name));
			inProgress = metricManager.CreateDeltaMetric (
				string.Format ("delta://network-requests/{0}/in-progress", name));
			inProgressSuccess = metricManager.CreateDeltaMetric(
				string.Format("delta://network-requests/{0}/in-progress-success", name));
		}

		public void Request ()
		{
			requested.Increment ();
			inProgress.Increment ();
			inProgressSuccess.Increment();
		}

		public void Response (int code)
		{
			completed.Increment ();
			inProgress.Decrement ();

			switch (code) {
				case 0:
					http0.Increment();
					break;
				case 200:
					http200.Increment();
					break;
				case 400:
					http400.Increment();
					break;
				case 404:
					http404.Increment();
					break;
				case 500:
					http500.Increment();
					break;
				default:
					if (code > 500)
						http5xx.Increment();
					break;
			}
		}

		public void Success()
		{
			success.Increment();
			inProgressSuccess.Decrement();
		}

	}
}
