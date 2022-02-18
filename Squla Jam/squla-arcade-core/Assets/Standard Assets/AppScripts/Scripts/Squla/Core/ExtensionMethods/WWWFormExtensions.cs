using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Squla.Core.ExtensionMethods
{
	public static class WWWFormExtensions
	{
		public static void AddFields (this WWWForm form, IDictionary<string, string> kvps)
		{
			if (kvps.Count == 0) {
				/* 
				 * Post requests with zero-sized post buffers 
				 * are not supported. Avoid this behaviour by adding a 
				 * "mock" field if the dictionary is empty
				 * 
				 * TODO: Check that this is always the desired behaviour
				 * For example this will obscure any error that might have 
				 * occured if the dictionary is unexpectedly empty making it
				 * harder to debug.
				*/
				form.AddField ("Empty", "data");
				return;
			}
			foreach (KeyValuePair<string, string> kvp in kvps) {
				form.AddField (kvp.Key, kvp.Value);
			}
		}
	}
}
