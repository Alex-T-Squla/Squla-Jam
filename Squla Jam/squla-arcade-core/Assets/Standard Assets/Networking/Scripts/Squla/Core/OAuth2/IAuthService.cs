using UnityEngine;
using System.Collections;
using SimpleJson;
using Squla.Core.Network;

namespace Squla.Core.Network
{
	public delegate void ApiSuccess (JsonObject jsonObject);
	public delegate void ApiError (int statusCode, JsonObject jsonObject);
	public delegate void GetAccessTokenResp (OAuth2Token token);

	public interface IAuthService
	{
		void InitializeOAuthToken (OAuth2Token token);

		void LoginWithUsernameAndPassword (string username, string password, ApiSuccess apiSuccess, ApiError apiError);

		void LoginWithGuest (ApiSuccess apiSuccess, ApiError apiError);

		void GetAccessToken (GetAccessTokenResp getAccessTokenResp, ApiError apiError);

		void SetAccessTokenExpired ();
	}
}