using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Squla.Core.ExtensionMethods;

namespace Squla.Core.Network
{
	internal static class RequestProcessor
	{
		private static readonly Regex PROTO = new Regex ("https?://");

		public static JsonRequest PreProcess (string apiBase, GETRequest request)
		{
			var newRequest = new JsonRequest {
				source = request,
				url = CreateURL (apiBase, request.href),
				background = request.background
			};

			return newRequest;
		}

		public static JsonRequest PreProcess (string apiBase, POSTRequest request)
		{
			if (request == null) {
				throw new Exception($"POSTRequest is null");
			}

			if (request.action == null) {
				throw new Exception($"request.action is null");
			}
			
			if (request.action.href == null) {
				throw new Exception($"request.action.href is null");
			}
			try {
				var newRequest = new JsonRequest {
					source = request,
					url = CreateURL(apiBase, request.action.href),
					formData = PrepareFormData(request),
					background = request.background
				};

				return newRequest;
			} catch (Exception e) {
				throw new Exception($"NullReferenceException POSTRequest apiBase {apiBase} request {request}", e);
			}
		}

		private static string CreateURL (string apiBase, string href)
		{
			return PROTO.IsMatch(href) ? href : apiBase + href;
		}

		private static byte[] PrepareFormData (POSTRequest request)
		{
			var form = new WWWForm ();

			var clone = new Dictionary<string, string> (request.action.@params);

			if (request.postParameters == null) {
				form.AddFields (clone);
				return form.data;
			}

			foreach (var keyvalue in request.postParameters) {
				clone [keyvalue.Key] = keyvalue.Value;
			}

			form.AddFields (clone);
			return form.data;
		}
	}
}

