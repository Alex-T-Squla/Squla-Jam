using System;
using System.Collections.Generic;
using Squla.Core.ObjectPooling;


namespace Squla.Core.Network
{
	public class DataRequest : IPoolableObject
	{
		public enum RequestType
		{
			Json,
			Image,
			Audio
		}

		private const string METHOD_GET = "GET";
		private const string METHOD_POST = "POST";

		/// <summary>
		/// Try counter will be incremented before taking a sleep for retry.
		/// </summary>
		public int tryCounter;

		public string url;

		public string requestId;

		public RequestType requestType = RequestType.Json;

		public readonly Dictionary<string, string> headers;

		public System.Object source;

		public string Method {
			get { return formData == null ? METHOD_GET : METHOD_POST; }
		}

		public RequestStatus Status { get; internal set; }

		public Action<DataRequest, IDataResponse> onComplete;

		public byte[] formData;

		public DataRequest ()
		{
			headers = new Dictionary<string, string> ();
		}

		public void Abort() 
		{
			if (Status == RequestStatus.Initial || 
			    Status == RequestStatus.Queued ||
			    Status == RequestStatus.Downloading) {
				
				Status = RequestStatus.Aborted;
			}
		}

		public virtual void Release ()
		{
			headers.Clear ();
			tryCounter = 0;
			url = string.Empty;
			formData = null;
			source = null;
			Status = RequestStatus.Initial;
			onComplete = null;
		}
	}
}
