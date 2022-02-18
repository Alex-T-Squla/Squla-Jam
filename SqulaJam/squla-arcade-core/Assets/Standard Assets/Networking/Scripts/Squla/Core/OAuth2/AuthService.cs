using UnityEngine;
using System;
using Squla.Core.Logging;
using Squla.Core.IOC;
using Squla.Core.ZeroQ;
using Squla.Core.i18n;

namespace Squla.Core.Network
{
	public class AuthService : IAuthService
	{
		private OAuth2Token token;
		private readonly SmartLogger logger = SmartLogger.GetLogger<AuthService> ();

		private readonly Bus bus;
		private readonly ScreenHUD hud;
		private readonly ILocaleConfiguration config;

		[Inject]
		public AuthService (Bus bus, ScreenHUD hud, ILocaleConfiguration config)
		{
			this.bus = bus;
			this.hud = hud;
			this.config = config;
		}

		public void InitializeOAuthToken (OAuth2Token token)
		{
			this.token = token;
		}

		public void GetAccessToken (GetAccessTokenResp getAccessTokenSuccess, ApiError apiError)
		{
//			logger.Debug ("-------GetAccessToken");
			if (token == null) {
				logger.Error ("-------Access token is null!");
			}
			// if about to expire then refresh it
			var currentTime = DateTime.Now;
			var expired = token.expires_at < currentTime;

			logger.Debug ("Token: Expires at: {0}, Cur. Time: {1}, Expired: {2}", token.expires_at, currentTime, expired);

			if (expired) {
				RefreshAccessToken (token, a => {
					getAccessTokenSuccess (token);
				}, (resCode, resObj) => {
					// this error is called only for 401
					apiError (resCode, resObj);  // key: 34f9e47ee2ed26976256780e0c5e4945
					bus.Publish ("cmd://oauth2/refresh-token/expired-step-1");
					bus.Publish ("cmd://oauth2/refresh-token/expired", resObj);
				});
			} else {
				getAccessTokenSuccess (token);
			}
		}

		public void SetAccessTokenExpired ()
		{
			token.expires_at = DateTime.MinValue;
		}

		public void LoginWithUsernameAndPassword (string username, string password, ApiSuccess apiSuccess, ApiError apiError)
		{
			logger.Debug ("-------LoginWithUsernameAndPassword");

			var form = new WWWForm ();
			form.AddField ("client_id", config.CLIENT_ID);
			form.AddField ("client_secret", config.CLIENT_SECRET);
			form.AddField ("username", username);
			form.AddField ("password", password);
			form.AddField ("grant_type", "password");

			LoginRequest (form, apiSuccess, apiError);
		}

		public void LoginWithGuest (ApiSuccess apiSuccess, ApiError apiError)
		{
			logger.Debug ("-------LoginWithGuest");

			var form = new WWWForm ();
			form.AddField ("client_id", config.CLIENT_ID);
			form.AddField ("client_secret", config.CLIENT_SECRET);
			form.AddField ("grant_type", "guest_access");

			LoginRequest (form, apiSuccess, apiError);
		}

		private void LoginRequest (WWWForm form, ApiSuccess apiSuccess, ApiError apiError)
		{
			var url = config.URL_LOGIN + "/oauth2/token";

			var request = new JsonRequest {
				url = url,
				formData = form.data
			};

			request.onComplete = (req, code, respJson) => {
				logger.Debug ("LoginRequest: {0} {1} {2}", code, req.url, respJson);
				if (code == 200) {
					token = new OAuth2Token (respJson);
					apiSuccess (respJson);
					return true;
				}
				if (code == 400) {
					apiError (code, respJson);
					return true;
				}
				return false;
			};

			hud.Execute (request);
		}

		private void RefreshAccessToken (OAuth2Token token, ApiSuccess apiSuccess, ApiError apiError)
		{
			var url = config.URL_LOGIN + "/oauth2/token";

			var form = new WWWForm ();
			form.AddField ("client_id", config.CLIENT_ID);
			form.AddField ("client_secret", config.CLIENT_SECRET);
			form.AddField ("refresh_token", token.refresh_token);
			form.AddField ("grant_type", "refresh_token");

			var request = new JsonRequest {
				url = url,
				formData = form.data,
				background = true
			};

			request.onComplete = (req, code, respJson) => {
				logger.Debug ("RefreshAccessToken: {0} {1} {2}", code, req.url, respJson);
				if (code == 200) {
					token.UpdateAccessToken (respJson);
					apiSuccess (respJson);
					bus.Publish ("cmd://oauth2/access-token/refreshed");
					return true;
				}
				if (code == 401) {
					apiError (code, respJson);
					return true;
				}
				return false;
			};

			hud.Execute (request);
		}
	}
}

