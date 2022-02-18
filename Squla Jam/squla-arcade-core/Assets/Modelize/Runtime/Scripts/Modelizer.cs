using System;
using System.Collections.Generic;
using SimpleJson;

namespace Squla.Core.Modelize
{
	public class Modelizer : IModelizer
	{
		private Dictionary<Type, TypeMetaClass> metaMap = new Dictionary<Type, TypeMetaClass> ();

		public Modelizer ()
		{
			metaMap [typeof(int)] = new PrimitiveMetaClass (typeof(int));
			metaMap [typeof(float)] = new PrimitiveMetaClass (typeof(float));
			metaMap [typeof(string)] = new PrimitiveMetaClass (typeof(string));
			metaMap [typeof(bool)] = new PrimitiveMetaClass (typeof(bool));
			metaMap [typeof(DateTime)] = new PrimitiveMetaClass (typeof(DateTime));
		}

		public void FlushCache<T> ()
		{
			GetMetaClass (typeof(T)).Flush ();
		}

		public void Modelize<T> (T target, JsonObject source)
		{
			Modelize (typeof(T), target, source);
		}

	    public void Modelize(System.Object target, JsonObject source)
	    {
	        Modelize(target.GetType(), target, source);
	    }

		public T Modelize<T> (JsonObject source)
		{
			return (T)Modelize (typeof(T), null, source);
		}

		public System.Object Modelize (Type targetType, System.Object target, System.Object source)
		{
			var metaClass = GetMetaClass (targetType);

			return metaClass.Modelize (target, source);
		}

		internal TypeMetaClass GetMetaClass (Type targetType)
		{
			if (!metaMap.ContainsKey (targetType))
				metaMap [targetType] = ObjectMetaClass.Create (this, targetType);

			return metaMap [targetType];
		}
	}
}

