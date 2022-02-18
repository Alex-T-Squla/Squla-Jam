using System;
using SimpleJson;

namespace Squla.Core.Modelize
{
	public class ObjectMetaClassModelizable: ObjectMetaClass
	{
		private CacheManager manager;

		public ObjectMetaClassModelizable (Modelizer context, Type targetType, ModelizeAttribute attr) : base (context, targetType)
		{
			// install cache manager.  That is the ctor resolver. this is stored in this instance.
			bool found = false;
			for (int i = 0; i < fields.Length; i++) {
				if (fields [i].FieldName == attr.KeyField) {
					found = true;
					break;
				}
			}

			if (!found) {
				throw new Exception (string.Format ("keyField {0} not defined in the model {1}", attr.KeyField, targetType));
			}

			manager = new CacheManager (targetType, attr.KeyField, attr.PoolSize);
		}

		protected override System.Object Resolve (JsonObject source)
		{
			return manager.Resolve (source);
		}

		public override void Flush ()
		{
			manager.Flush ();
		}
	}
}
