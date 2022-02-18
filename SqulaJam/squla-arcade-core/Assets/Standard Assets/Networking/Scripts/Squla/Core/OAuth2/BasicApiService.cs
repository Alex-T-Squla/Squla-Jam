using UnityEngine;
using Squla.Core.IOC;
using Squla.Core.i18n;
using SimpleJson;

namespace Squla.Core.Network
{
	public class BasicApiService : IApiService
	{
		private readonly ScreenHUD screenHUD;
		private readonly ILocaleConfiguration config;
		internal IResponseCodePolicy responsePolicy;
		private string authorization;
		private string invalidator;

		[Inject]
		public BasicApiService (ScreenHUD screenHUD, ILocaleConfiguration config)
		{
			this.screenHUD = screenHUD;
			this.config = config;
		}

		public void GET (string url, ApiSuccess apiSuccess, ApiError apiError = null)
		{
			var request = new GETRequest {
				href = url,
				onSuccess = apiSuccess,
				onError = apiError
			};
			GET (request);
		}

		public void POST(string url, byte[] data, string contentType, ApiSuccess apiSuccess, ApiError apiError = null)
		{
			// Do nothing
		}

		public void GET (GETRequest request)
		{
			var newRequest = RequestProcessor.PreProcess (config.URL_API, request);
			Execute (request, newRequest);
		}

		public void POST (POSTRequest request)
		{
			var newRequest = RequestProcessor.PreProcess (config.URL_API, request);
			Execute (request, newRequest);
		}

		private void Execute (Request origRequest, JsonRequest request)
		{
			if (origRequest.onAbort != null) {
				screenHUD.OnAbort(() => origRequest.onAbort());
			}

			if (invalidator != config.CLIENT_SECRET) {
				invalidator = config.CLIENT_SECRET;
				authorization = "Basic " + System.Convert.ToBase64String (System.Text.Encoding.ASCII.GetBytes (config.CLIENT_ID + ":" + config.CLIENT_SECRET));
			}

			request.authorization = authorization;
			request.onComplete = OnExecuteComplete;

			screenHUD.Execute (request);
		}

		private bool OnExecuteComplete (JsonRequest request, int code, JsonObject respJson)
		{
			if (!responsePolicy.CanHandle (code))
				return false;

			var origRequest = (Request)request.source;
			if (code == 200) {
				origRequest.onSuccess (respJson);
			} else if (origRequest.onError != null) {
				if (origRequest.onError == null)
					return false;
				origRequest.onError (code, respJson);
			}

			return true;
		}
	}
}
