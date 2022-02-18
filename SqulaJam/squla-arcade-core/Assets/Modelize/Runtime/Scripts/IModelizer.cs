using System;
using SimpleJson;

namespace Squla.Core.Modelize
{
	public interface IModelizer
	{
		void Modelize<T> (T target, JsonObject source);

	    void Modelize(System.Object target, JsonObject source);

		T Modelize<T> (JsonObject source);

		void FlushCache<T> ();
	}
}

