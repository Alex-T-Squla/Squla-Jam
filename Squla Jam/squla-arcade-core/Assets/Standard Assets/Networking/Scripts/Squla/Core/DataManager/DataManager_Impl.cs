using System;
using SimpleJson;
using Squla.Core.IOC;
using Squla.Core.Modelize;
using Squla.Core.Network;
using Squla.Core.ZeroQ;

namespace Squla.Core.DataManager
{
    public class DataManager_Impl : IDataManager
    {
	    public const string cmd_Pre_Fetched_Data = "cmd://pre-fetched-data";

        private readonly DataSpecManager dataSpecManager;
        private readonly Cached_API_Manager apiManager;

        public DataManager_Impl (ObjectGraph graph, Bus bus, IModelizer modelizer, IApiService getService, IApiService postService)
        {
	        bus.Register (this);
            apiManager = new Cached_API_Manager(getService, postService);
            dataSpecManager = new DataSpecManager(graph, modelizer);
            dataSpecManager.DetectAndCreateDataSpecs();
        }

	    [Subscribe (cmd_Pre_Fetched_Data)]
	    public void WssPreFetchedData (JsonObject jsonObject)
	    {
		    dataSpecManager.ProcessPreFetchedData(jsonObject);
	    }

        public void Flush ()
        {
	        apiManager.Flush ();
	        dataSpecManager.Flush ();
        }

        public void ProcessPreFetchedData(JsonObject jsonObject)
        {
	        dataSpecManager.ProcessPreFetchedData(jsonObject);
        }

        public void GET (GETRequest request, ApiSuccess onSuccess=null)
        {
	        if (!request.no_cache) {
		        var obj = dataSpecManager.Sync_GET(request.href);
		        if (obj != null) {
			        return;
		        }
	        }
            apiManager.GET(request, jsonObject => {
	            dataSpecManager.ProcessPreFetchedData(jsonObject);
                dataSpecManager.ProcessResponse(request.href, jsonObject, obj => onSuccess?.Invoke (jsonObject));
            });
        }

	    public void GET<T> (GETRequest request, System.Action<T> onResponse)
	    {
		    if (!request.no_cache) {
				var obj = dataSpecManager.Sync_GET(request.href);
				if (obj is T) {
					onResponse((T) obj);
					return;
				}
		    }

		    apiManager.GET(request, jsonObject => {
			    dataSpecManager.ProcessPreFetchedData(jsonObject);
			    dataSpecManager.ProcessResponse(request.href, jsonObject, obj => {
				    if (obj is T) {
					    onResponse((T) obj);
				    } else {
					    throw new InvalidCastException($"Invalid cast exception! Trying to convert {obj.GetType().Name} to {typeof(T).Name} from request {request.href}");
				    }
			    });
		    });
	    }

	    public T CacheOnly_GET<T>(string url)
	    {
		    var obj = dataSpecManager.Sync_GET(url);
		    if (obj == null)
			    return default(T);

		    if (obj is T ) {
			    return (T)obj;
		    } else {
			    throw new InvalidCastException($"Invalid cast exception! Trying to convert {obj.GetType().Name} to {typeof(T).Name} from request {url}");
		    }
	    }

        public void POST (POSTRequest request)
        {
	        var origSuccess = request.onSuccess;
	        request.onSuccess = jsonObject => {
		        dataSpecManager.ProcessPreFetchedData(jsonObject);
		        origSuccess(jsonObject);
	        };
            apiManager.POST(request);
        }
    }
}
