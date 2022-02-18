using System;
using UnityEngine;
using Squla.Core.IOC;
using Squla.Core.i18n;
using Squla.Core.Logging;

namespace Squla.Core.Network
{
	public class OAuth2ApiService : IApiService
	{
		private readonly SmartLogger logger;

		private readonly IAuthService authService;

		private readonly ScreenHUD screenHUD;

		private readonly ILocaleConfiguration config;

		internal IResponseCodePolicy responsePolicy;

		private string authorization;
		private string invalidator;

		[Inject]
		public OAuth2ApiService (IAuthService authService, ScreenHUD screenHUD, ILocaleConfiguration config)
		{
			logger = SmartLogger.GetLogger<OAuth2ApiService> ();

			this.authService = authService;
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
			try {
				var request = new POSTRequest {
					action = new ApiAction {href = url},
					onSuccess = apiSuccess,
					onError = apiError
				};
				var newRequest = RequestProcessor.PreProcess(config.URL_API, request);
				newRequest.formData = data;
				newRequest.contentType = contentType;
				Execute(request, newRequest);
			} catch (Exception e) {
				throw new Exception($"POST Exception with url {url}", e);
			}
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
				screenHUD.OnAbort (origRequest.onAbort);
			}

			logger.Debug ("ExecuteApiCall: {0} {1}", request.Method, request.url);

			request.onComplete = (req, code, respJson) => {
				if (code == 401) {
					// this is very rare chance.
					// set the access token to expire.
					authService.SetAccessTokenExpired ();
					// then recursively call self.
					_Execute (request);
					return true;
				}

				if (!responsePolicy.CanHandle (code))
					return false;

				logger.Debug ("Response: {0} {1} {2}", code, req.url, respJson);
				if (code == 200) {
					origRequest.onSuccess (respJson);
				} else {
					if (origRequest.onError == null)
						return false;
					origRequest.onError (code, respJson);
				}

				return true;
			};

			_Execute (request);
		}

		private void _Execute (JsonRequest request)
		{
			// get accessToken
			authService.GetAccessToken (a => {

				if (a.access_token != invalidator) {
					authorization = "Bearer " + a.access_token;
					invalidator = a.access_token;
				}

				logger.Debug ("access-token {0}", authorization);
				request.authorization = authorization;

				screenHUD.Execute (request);
			}, (code, respJson) => {
				logger.Debug ("GetAccessToken Error: {0}:{1}", code, respJson);
				if (code == 401) {
					// refresh token expired. do not call the request response. end the transaction.
				}
			});
		}
	}
}
