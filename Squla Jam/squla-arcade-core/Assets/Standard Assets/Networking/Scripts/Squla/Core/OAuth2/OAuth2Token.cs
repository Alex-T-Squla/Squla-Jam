using UnityEngine;
using System.Collections;
using SimpleJson;
using System;

namespace Squla.Core.Network
{
	public class OAuth2Token
	{
		public string access_token;
		public string refresh_token;
		public string token_type;

		public DateTime expires_at;

		public OAuth2Token (string access_token, string refresh_token, int expires_in)
		{
			this.access_token = access_token;
			this.refresh_token = refresh_token;
			this.token_type = "Bearer";
			this.expires_at = DateTime.Now.AddSeconds (expires_in);
		}

		public OAuth2Token (string access_token, string refresh_token, DateTime expires_at)
		{
			this.access_token = access_token;
			this.refresh_token = refresh_token;
			this.token_type = "Bearer";
			this.expires_at = expires_at;
		}

		public OAuth2Token (JsonObject jsonObj)
		{
			var token = new OAuth2Token ((string)jsonObj ["access_token"], (string)jsonObj ["refresh_token"], Convert.ToInt32 (jsonObj ["expires_in"]));

			this.access_token = token.access_token;
			this.refresh_token = token.refresh_token;
			this.token_type = "Bearer";
			this.expires_at = token.expires_at;
		}

		public void UpdateAccessToken (JsonObject jsonObj)
		{
			var token = new OAuth2Token ((string)jsonObj ["access_token"], null, Convert.ToInt32 (jsonObj ["expires_in"]));

			this.access_token = token.access_token;
			this.expires_at = token.expires_at;
		}

		public OAuth2Token ()
		{
			
		}
	}
}
