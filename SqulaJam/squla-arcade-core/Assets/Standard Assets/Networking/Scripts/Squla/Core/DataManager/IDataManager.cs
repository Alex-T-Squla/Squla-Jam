using SimpleJson;

namespace Squla.Core.Network
{
    public interface IDataManager
    {
	    /// <summary>
	    /// Recommended to flush the cache on logout
	    /// </summary>
	    void Flush ();

	    void ProcessPreFetchedData(JsonObject jsonObject);
	    
        void GET (GETRequest request, ApiSuccess onSuccess=null);

	    void GET<T> (GETRequest request, System.Action<T> onResponse);

	    /// <summary>
	    /// Use with caution. if the requested url not found in the cache. you will get into rabbit hole.
	    /// It expects the response to already be stored in the cache
	    /// </summary>
	    /// <param name="url"></param>
	    /// <typeparam name="T"></typeparam>
	    /// <returns></returns>
	    T CacheOnly_GET<T>(string url);

        void POST (POSTRequest request);
    }
}