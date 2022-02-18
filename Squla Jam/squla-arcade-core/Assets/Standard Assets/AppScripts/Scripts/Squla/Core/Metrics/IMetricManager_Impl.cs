using System;
using System.Collections.Generic;
using System.IO;
using SimpleJson;
using UnityEngine;

namespace Squla.Core.Metrics
{
    public class IMetricManager_Impl : IMetricManager
    {
        private readonly Dictionary<string, object> metrics = new Dictionary<string, object>();
        private readonly Queue<string> metricLog = new Queue<string>();
        private readonly HashSet<NetworkMetric_Impl> pending = new HashSet<NetworkMetric_Impl>();

        private StreamWriter metricStream;

        private int requestId;
        private bool collect;
        private string uniqueId;
        private int writtenCount;

        private Action<string, string> exceptionCallback;
        private readonly Queue<ExceptionLite> exceptionQueue = new Queue<ExceptionLite> ();

        public string MetricsDataPath { get; private set; }

        public IMetricManager_Impl()
        {
            MetricsDataPath = Application.persistentDataPath + "/Metrics";
            collect = true;
            Application.logMessageReceived += HandleException;
            Started();
        }

        private void HandleException (string condition, string stackTrace, LogType type)
        {
            if (!(type == LogType.Exception || type == LogType.Error))
                return;

            condition = condition ?? "";
            stackTrace = stackTrace ?? "";
            var error = condition + Environment.NewLine + stackTrace;

            var es = DateTime.Now.ToString ("yyyy-MM-dd HH:mm:ss,fff");
            ReportException (es, error);

            if (!collect)
	            return;

            var data = new Dictionary<string, string> {
                {"t", es},
                {"m", condition + Environment.NewLine + stackTrace}
            };

            var msg = SimpleJSON.SerializeObject(data).Replace("\\n", "\\\\n");
            metricLog.Enqueue(msg);
        }

        private void ReportException (string t, string error)
        {
	        if (exceptionCallback != null) {
		        exceptionCallback (t, error);
		        return;
	        }

	        exceptionQueue.Enqueue (new ExceptionLite {time=t, error=error});
        }

        public void RegisterOnException (Action<string, string> onException)
        {
	        exceptionCallback = onException;
	        while (exceptionQueue.Count > 0) {
		        var item = exceptionQueue.Dequeue ();
		        ReportException (item.time, item.error);
	        }
        }

        public CountMetric CreateCountMetric(string name)
        {
            if (!metrics.ContainsKey(name)) {
                metrics.Add(name, new CountMetric());
            }

            var value = metrics[name];
            if (value.GetType().FullName != typeof(CountMetric).FullName)
                throw new Exception("type associated with the name is different");

            return (CountMetric) value;
        }

        public DeltaMetric CreateDeltaMetric(string name)
        {
            if (!metrics.ContainsKey(name)) {
                metrics.Add(name, new DeltaMetric());
            }

            var value = metrics[name];
            if (value.GetType().FullName != typeof(DeltaMetric).FullName)
                throw new Exception("type associated with the name is different");

            return (DeltaMetric) value;
        }

        public NetworkMetric CreateNetworkMetric(string http_verb, string url, int tryCount)
        {
            var metric = new NetworkMetric_Impl(this, http_verb, url, tryCount, ++requestId);
            pending.Add(metric);
            return metric;
        }

        private void Started()
        {
            try {
                if (!Directory.Exists(MetricsDataPath))
                    Directory.CreateDirectory(MetricsDataPath);
            }
            catch (Exception) {}

            LogEvent (new Dictionary<string, string> {
                {"event_name", "app-bootstrap"},
                {"device_model", SystemInfo.deviceModel},
                {"os", SystemInfo.operatingSystem},
                {"memory", SystemInfo.systemMemorySize.ToString()},
                {"build_id", Application.buildGUID}
            });
        }

        public void Cleanup()
        {
            if (metricStream == null)
                return;

            try {
                metricStream.Close();
            }
            catch (Exception) {
            }
            finally {
                writtenCount = 0;
                metricStream = null;
            }
        }

        private void CreateStream()
        {
            try {
                var dt = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var fileName = string.Format("{0}/network_metric-{1}-{2}.log", MetricsDataPath, uniqueId, dt);
                metricStream = File.CreateText(fileName);
                metricStream.AutoFlush = true;
                writtenCount = 0;
            }
            catch (Exception) {
                metricStream = null;
            }
        }

        public void Init(string userId, bool collect)
        {
            Cleanup();

            this.collect = collect;
            if (!this.collect) {
                return;
            }

            uniqueId = userId;
            CreateStream();
        }

        public bool Flush()
        {
            while (metricLog.Count > 0 && metricStream != null) {
                try {
                    var row = metricLog.Peek();
                    metricStream.WriteLine(row);
                    metricLog.Dequeue();
                    writtenCount++;
                }
                catch (Exception) {
                    break;
                }
            }

            return writtenCount > 75;
        }

        public void ApplicationPaused()
        {
            foreach (var item in pending) {
                item.FreezeResponseTime();
            }

            Flush();
        }

        public void LogEvent(IDictionary<string, string> data)
        {
            if (!collect)
                return;

            data = new Dictionary<string, string>(data);
            data["t"] = DateTime.Now.ToString ("yyyy-MM-dd HH:mm:ss,fff");
            var msg = SimpleJSON.SerializeObject(data);
            metricLog.Enqueue(msg);
        }

        private void LogMetric(NetworkMetric_Impl metric)
        {
            pending.Remove(metric);

            if (collect && metric.MayLog) {
                metricLog.Enqueue(metric.ToString());
            }
        }

        private class NetworkMetric_Impl: NetworkMetric
        {
            private readonly string http_verb;
            private readonly string url;
            private readonly int tryCount;
            public int requestId { get; private set; }
            private readonly DateTime startDt;
            private int httpStatus;
            private int bytesCount;
            private DateTime endDt;
            private int responseTimeInMilliSeconds;
            private bool computed;
            private string extra_properties;

            private IMetricManager_Impl manager;

            public bool MayLog {
                get {
//                    if (!manager.mediaDomain.IsMatch(url))
//                        return true;

//                    return responseTimeInMilliSeconds > 3000;
                    return true;
                }
            }

            public NetworkMetric_Impl(IMetricManager_Impl manager, string http_verb, string url, int tryCount, int requestId)
            {
                this.manager = manager;
                this.http_verb = http_verb;
                this.url = url;
                this.tryCount = tryCount;
                this.requestId = requestId;

                startDt = DateTime.Now;
                httpStatus = 0;
                bytesCount = 0;
                responseTimeInMilliSeconds = 0;
                extra_properties = string.Empty;
            }

            public override string ToString()
            {
                var u = new Uri(url);

                var es = endDt.ToString ("yyyy-MM-dd HH:mm:ss,fff");
                var str = string.Format("'t': '{0}', 'u': '{1} {2}', 'try': {3}, 'id': {4}, 'h': {5}, 'rt': {6}, 's': {7}{8}",
                    es, http_verb, u.PathAndQuery, tryCount, requestId, httpStatus, responseTimeInMilliSeconds, bytesCount, extra_properties);
                return string.Format("{{{0}}}", str.Replace("'", "\""));
            }

            public void FreezeResponseTime()
            {
                if (computed)
                    return;

                endDt = DateTime.Now;
                var dt = endDt - startDt;
                responseTimeInMilliSeconds = dt.TotalMilliseconds < 0 ? -1 : (int)dt.TotalMilliseconds;
                computed = true;
            }

            public void Response(int httpStatus, int bytesCount)
            {
                this.httpStatus = httpStatus;
                this.bytesCount = bytesCount;

                FreezeResponseTime();

                if (manager != null)
                    manager.LogMetric(this);
                manager = null;
            }

            public void Response (int httpStatus, int bytesCount, int serverMilliseconds)
            {
                this.httpStatus = httpStatus;
                this.bytesCount = bytesCount;
                extra_properties = string.Format (", 'ns_rt': {0}", serverMilliseconds);

                FreezeResponseTime();

                if (manager != null)
                    manager.LogMetric(this);
                manager = null;
            }

            public void Response (int httpStatus, int bytesCount, int nativeMilliseconds, int serverMilliseconds)
            {
                this.httpStatus = httpStatus;
                this.bytesCount = bytesCount;
                extra_properties = string.Format (", 'nc_rt': {0}, 'ns_rt': {1}", nativeMilliseconds, serverMilliseconds);

                FreezeResponseTime();

                if (manager != null)
                    manager.LogMetric(this);
                manager = null;
            }
        }

        private class ExceptionLite
        {
	        public string time;
	        public string error;
        }
    }
}
