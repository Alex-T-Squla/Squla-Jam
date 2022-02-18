using System;
using System.Collections.Generic;

namespace Squla.Core.Metrics
{
    public interface IMetricManager
    {
	    void RegisterOnException(Action<string, string> onException);

        string MetricsDataPath { get; }

        void Cleanup();

        void Init(string userId, bool collect);

        bool Flush();

        void ApplicationPaused();

        CountMetric CreateCountMetric(string name);

        DeltaMetric CreateDeltaMetric(string name);

        NetworkMetric CreateNetworkMetric(string http_verb, string url, int tryCount);

        void LogEvent(IDictionary<string, string> data);
    }
}
