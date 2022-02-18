using System;
using SimpleJson;

namespace Squla.Core.Modelize
{
	public class ObjectMetaClassDefault : ObjectMetaClass
	{
		public ObjectMetaClassDefault (Modelizer context, Type targetType) : base (context, targetType)
		{
		}

		protected override System.Object Resolve (JsonObject source)
		{
			return Activator.CreateInstance (targetType);
		}
	}
}

