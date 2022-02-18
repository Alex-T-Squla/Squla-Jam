using System;
using System.Collections;
using SimpleJson;
using System.Reflection;

namespace Squla.Core.Modelize
{
	public class PropertyMetaClassDictionary : PropertyMetaClass
	{
		private Type valueType;

		public PropertyMetaClassDictionary (Modelizer context, FieldInfo fieldInfo) : base (context, fieldInfo)
		{
			valueType = fieldInfo.FieldType.GetGenericArguments () [1];
		}

		// Only handle Dictionary<String, String> for now and let it fail otherwise.
		public override void SetValue (System.Object target, System.Object value)
		{
			var newValue = (JsonObject)value;
			var targetValue = (IDictionary)fieldInfo.GetValue (target);
			targetValue.Clear ();

			foreach (var item in newValue) {
				var modelizedValue = context.Modelize(valueType, null, item.Value);
				targetValue.Add (item.Key, modelizedValue);
			}
		}
	}
}

