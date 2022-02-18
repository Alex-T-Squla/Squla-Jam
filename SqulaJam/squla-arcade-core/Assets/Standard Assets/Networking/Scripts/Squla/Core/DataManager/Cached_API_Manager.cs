using System.Collections.Generic;
using SimpleJson;
using Squla.Core.Network;

namespace Squla.Core.DataManager
{
    internal class Cached_API_Manager
    {
        private readonly Dictionary<string, CacheEntry> cache = new Dictionary<string, CacheEntry>();

        private readonly IApiService getService;
        private readonly IApiService postService;

        public Cached_API_Manager(IApiService getService, IApiService postService)
        {
            this.getService = getService;
            this.postService = postService;
        }

        public void Flush ()
        {
	        cache.Clear ();
        }

        public void GET(GETRequest request, ApiSuccess apiSuccess)
        {
	        var useCache = request.no_cache == false;
            if (cache.ContainsKey(request.href) && useCache) {
                var entry = cache[request.href];
                if (entry.last_modified == request.last_accessed) {
                    // do nothing.
                    return;
                }
                // if entry.etag is null or empty, then make a request.
                request.last_accessed = entry.last_modified;
                apiSuccess(entry.data);
                return;
            }

            request.onSuccess = jsonObject => {
                // this jsonObject is not cached at response level.
                ProcessPreFetchedData(jsonObject);
	            UpdateCache (request.href, "client", jsonObject);
                apiSuccess(jsonObject);
            };

            getService.GET(request);
        }

	    public JsonObject Sync_GET(string url)
	    {
		    return cache.ContainsKey(url) ? cache[url].data : null;
	    }

        public void POST(POSTRequest request)
        {
            var origSuccess = request.onSuccess;
            request.onSuccess = jsonObject => {
                ProcessPreFetchedData(jsonObject);
                origSuccess(jsonObject);
            };

            postService.POST(request);
        }

        private void ProcessPreFetchedData(JsonObject jsonObject)
        {
            if (!jsonObject.ContainsKey(Constants.k_PRE_FETCHED_DATA))
                return;

            var asList = (JsonArray) jsonObject[Constants.k_PRE_FETCHED_DATA];

            for (var i = 0; i < asList.Count; i++) {
                var item = (JsonObject) asList[i];
                var href = (string) item[Constants.k_HREF];
                var lastModified = (string) item[Constants.k_LAST_MODIFIED];
                UpdateCache(href, lastModified, (JsonObject) item[Constants.k_DATA]);
            }
        }

        private CacheEntry UpdateCache(string href, string lastModified, JsonObject data)
        {
            CacheEntry entry;
	        var needUpdate = true;
            if (!cache.TryGetValue(href, out entry)) {
                entry = new CacheEntry();
	            entry.data = data;
                cache.Add(href, entry);
	            needUpdate = false;
            }

            entry.last_modified = lastModified;
	        if (needUpdate) {
		        foreach (var pair in data) {
			        entry.data [pair.Key] = pair.Value;
		        }
	        }

	        return entry;
        }

        private class CacheEntry
        {
            public string last_modified;
            public JsonObject data;
        }
    }
}