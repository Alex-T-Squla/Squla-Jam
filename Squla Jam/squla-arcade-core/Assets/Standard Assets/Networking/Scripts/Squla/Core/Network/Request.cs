using System;
using System.Collections.Generic;

namespace Squla.Core.Network
{
	public abstract class Request
	{
		public string transactionID;
		public bool background;
		public ApiSuccess onSuccess;
		public ApiError onError;
		public Action onAbort;
		public AssetInfo[] assetInfos;
		public bool IsAborted { get; private set; }

		public void Abort()
		{
			IsAborted = true;
		}
	}

	public class GETRequest : Request
	{
		public string href;
	    public string last_accessed;
		public bool no_cache;

	    public GETRequest Clone()
	    {
	        return new GETRequest {
	            transactionID = transactionID,
	            background = background,
	            onSuccess = onSuccess,
	            onError = onError,
	            onAbort = onAbort
	        };
	    }
	}

	public class GETTypedRequest : GETRequest
	{
		public string href_type;
	}

	public class POSTRequest : Request
	{
		public ApiAction action;
		public Dictionary<string, string> postParameters = new Dictionary<string, string> ();
	}
}
