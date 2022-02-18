using Squla.Core.IOC;
using Squla.Core.Metrics;
using UnityEngine.UI;

namespace Squla.Core.Tests
{
	public class RequestMetricView : MonoBehaviourV2
	{
		public Text requestedCount;
		public Text completedCount;
		public Text http404Count;
		public Text http0Count;
		public Text inProgressCount;

		[Inject]
		private IMetricManager metricManager;

		private RequestMetrics metrics;

		public void Init(string metricName)
		{
			metrics = new RequestMetrics (metricName, metricManager);
		}

		void Update ()
		{
			if (metrics == null)
				return;

			requestedCount.text = string.Format ("Requested: {0}", metrics.requested.Count);
			completedCount.text = string.Format ("Completed: {0}", metrics.completed.Count);
			http404Count.text = string.Format ("NotFound: {0}", metrics.http404.Count);
			http0Count.text = string.Format ("NoConnction: {0}", metrics.http0.Count);
			inProgressCount.text = string.Format ("InProgress: {0}", metrics.inProgress.Count);
		}
	}
}
