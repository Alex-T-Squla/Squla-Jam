using System;
using SimpleJson;

namespace Squla.Core.Modelize
{
	public class CacheKey
	{
		private string keyProperty;

		public CacheKey (string keyProperty)
		{
			this.keyProperty = keyProperty;
		}

		public string Resolve (JsonObject source)
		{
			return source.ContainsKey (keyProperty) ? source [keyProperty].ToString () : null;
		}
	}
}

