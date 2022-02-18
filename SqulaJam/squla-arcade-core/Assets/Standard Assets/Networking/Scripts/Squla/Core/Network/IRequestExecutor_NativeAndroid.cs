using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJson;
using Squla.Core.IOC;
using Squla.Core.Metrics;
using Squla.Core.Modelize;
using UnityEngine;
using UnityEngine.Networking;

namespace Squla.Core.Network
{
	/// <summary>
	/// No [Sigleton] for this class.
	/// </summary>
	public class IRequestExecutor_NativeAndroid: MonoBehaviourV2, IRequestExecuter
	{
		private const bool mayUseHTTP2 = false;

		[Inject]
		private IMetricManager metricManager;

		[Inject]
		private IModelizer modelizer;

		private AndroidJavaObject nativeTarget;
		private Dictionary<string, Wrapper> pendingRequests;
		private string basePath;
		private readonly IDataResponse nullResponse = new IDataResponse_Null (0);

		protected override void OnEnable ()
		{
			base.OnEnable ();
			logger.Info("----OnEnable called----");
			pendingRequests = new Dictionary<string, Wrapper> ();
			nativeTarget = new AndroidJavaObject("com.squla.requestexecutor.HttpRequestExecutor");
			InitDirectories();
		}

		private void InitDirectories()
		{
			basePath = Application.persistentDataPath + "/NativeRequests";
			if (Directory.Exists (basePath)) {
				Directory.Delete (basePath, true);
			}
			Directory.CreateDirectory (basePath);
		}

		public void Execute (DataRequest request)
		{
			var nativeRequest = PrepareNativeRequest (request);

			var body = new byte[0];
			if (request.formData != null) {
				body = request.formData;
			}

			nativeTarget.Call ("execute", nativeRequest, body, mayUseHTTP2);
		}

		private void Update()
		{
			if (pendingRequests.Count == 0)
				return;

			var jsonResp = nativeTarget.Call<string>("peekResponse");
			if (string.IsNullOrEmpty (jsonResp))
				return;

			var json = (JsonObject) SimpleJSON.DeserializeObject (jsonResp);
			var resp = modelizer.Modelize<NativeResponse> (json);

			if (!pendingRequests.ContainsKey (resp.id)) {
				logger.Error ("Multiple response for the same request {0} {1}", resp.id, resp.url);
				return;
			}

			StartCoroutine(OnNativeAPIResponse(resp));
		}

		private IEnumerator OnNativeAPIResponse (NativeResponse resp)
		{
			var wrapper = pendingRequests[resp.id];
			pendingRequests.Remove (resp.id);
			var request = wrapper.request;
			var metric = wrapper.metric;
			IDataResponse response = nullResponse;

			var www = new UnityWebRequest("file://" + wrapper.fileName) {
				downloadHandler = createDownloadHandler(request)
			};
			yield return www.SendWebRequest();

			metric.Response (resp.http_status, resp.bytes_count, resp.client_response_time_in_ms, resp.server_response_time_in_ms);

			if (request.onComplete == null) {
				logger.Debug("DataRequest.onComplete is null for {0}", request.url);
			} else {
				// if the original response http status is 0 then we should send nullResponse
				if (resp.http_status != 0)
					response = new IDataResponse_WebRequest (www, resp.http_status);
				request.onComplete(request, response);
			}
			File.Delete(wrapper.fileName);
		}

		private string PrepareNativeRequest (DataRequest request)
		{
			request.tryCounter++;
			request.Status = RequestStatus.Downloading;

			var metric = metricManager.CreateNetworkMetric(request.Method, request.url, request.tryCounter);

			logger.Debug ("Downloading {0} {1} try: {2}", request.Method, request.url, request.tryCounter);

			var headers = new NativeRequestHeader[request.headers.Count];
			var i = 0;
			foreach (var header in request.headers) {
				headers[i] = new NativeRequestHeader (header);
				i++;
			}

			var fileName = basePath + "/" + metric.requestId + ".bin";
			var req = new NativeRequest {
				id = metric.requestId.ToString (),
				url = request.url,
				respFilePath = fileName,
				headers = headers
			};

			var wrapper = new Wrapper {
				request = request,
				metric = metric,
				fileName = fileName
			};

			pendingRequests.Add (req.id, wrapper);

			return SimpleJSON.SerializeObject (req);
		}

		private  DownloadHandler createDownloadHandler(DataRequest request)
		{
			if (request.requestType == DataRequest.RequestType.Audio)
				return new DownloadHandlerAudioClip(request.url, AudioType.MPEG);

			return new DownloadHandlerBuffer();
		}

		private class Wrapper
		{
			public DataRequest request;
			public NetworkMetric metric;
			public string fileName;
		}

		private class NativeRequest
		{
			public string id;
			public string url;
			public string respFilePath;
			public NativeRequestHeader[] headers;
		}

		private class NativeRequestHeader
		{
			// ReSharper disable once MemberCanBePrivate.Local
			public string name;

			// ReSharper disable once MemberCanBePrivate.Local
			public string value;

			public NativeRequestHeader (KeyValuePair<string, string> src)
			{
				name = src.Key;
				value = src.Value;
			}
		}

		// ReSharper disable once ClassNeverInstantiated.Local
		private class NativeResponse
		{
			public string id;
			public string url;
			public int http_status;
			public int bytes_count;
			public int server_response_time_in_ms;
			public int client_response_time_in_ms;
			public string content;
		}

		private class IDataResponse_NativeAndroid : IDataResponse
		{
			public int ResponseCode { get; private set; }

			public string ResponseText { get; private set; }

			public Sprite Sprite {
				get { return null; }
			}

			public AudioClip AudioClip {
				get { return null; }
			}

			public byte[] AudioBytes {
				get { return null; }
			}

			internal IDataResponse_NativeAndroid (int code, string content)
			{
				ResponseCode = code;
				ResponseText = content;
			}
		}
	}
}
