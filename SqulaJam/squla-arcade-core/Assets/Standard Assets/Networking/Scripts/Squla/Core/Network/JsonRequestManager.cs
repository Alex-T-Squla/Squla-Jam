using Squla.Core.IOC;
using SimpleJson;
using System;
using Squla.Core.Metrics;

namespace Squla.Core.Network
{
	public class JsonRequest
	{
		private const string kGET = "GET";
		private const string kPOST = "POST";

		public System.Object source;
		public bool background;

		public string url;
		public string authorization;
		public string contentType;
		public byte[] formData;
		public Func<JsonRequest, int, JsonObject, bool> onComplete;

		public bool IsAborted { get; private set; }

		public string Method { get { return formData != null ? kPOST : kGET; } }

		public void Abort ()
		{
			IsAborted = true;
		}
	}

	[Singleton]
	public class JsonRequestManager
	{
		private const string kXRequestId = "X-Request-Id";
		private const string kUserAgent = "User-Agent";
		private const string kAuthorization = "Authorization";
		private const string kContentType = "Content-Type";
		private const string kXDeviceId = "X-Device-Id";

		private readonly INetworkManager manager;

		private readonly RequestMetrics metrics;

		private int requestId;
		private readonly string userAgent;
		private readonly string deviceId;

		[Inject]
		public JsonRequestManager ([Inject ("JSON")]INetworkManager networkManager, IConfiguration config, IMetricManager metricManager)
		{
			manager = networkManager;
			userAgent = config.UserAgent;
			deviceId = config.DeviceId_b64;
			metrics = new RequestMetrics ("json", metricManager);
		}

		public void Execute (JsonRequest request)
		{
			metrics.Request ();

			var dataRequest = new DataRequest { 
				url = request.url,
				formData = request.formData,
				source = request,
				onComplete = Response
			};

			dataRequest.headers [kXRequestId] = dataRequest.requestId = (++requestId).ToString ();
			dataRequest.headers [kUserAgent] = userAgent;
			dataRequest.headers [kXDeviceId] = deviceId;
			if (!string.IsNullOrEmpty (request.authorization)) {
				dataRequest.headers [kAuthorization] = request.authorization;
			}

			if (!string.IsNullOrEmpty (request.contentType)) {
				dataRequest.headers [kContentType] = request.contentType;
			}

			manager.Execute (dataRequest);
		}

		private void Response (DataRequest req, IDataResponse resp)
		{
			metrics.Response (resp.ResponseCode);

			var request = req.source as JsonRequest;
			var responseText = resp.ResponseText;

			if (request.IsAborted)
				return;

			JsonObject jsonObject;
			if (resp.ResponseCode > 0 && resp.ResponseCode < 500) {
				if (string.IsNullOrEmpty(responseText) || responseText[0] != '{') {
					jsonObject = new JsonObject {
						{"responseText", responseText}
					};
				} else {
					jsonObject = (JsonObject)SimpleJSON.DeserializeObject(responseText);
				}
			} else {
				jsonObject = new JsonObject ();
			}

			request.onComplete (request, resp.ResponseCode, jsonObject);
			metrics.Success();
		}
	}

}
