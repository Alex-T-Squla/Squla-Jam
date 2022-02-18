using System;
using System.Collections;
using System.Reflection;
using SimpleJson;

namespace Squla.Core.Modelize
{
	public class PropertyMetaClassArray: PropertyMetaClass
	{
		private Type elementType;

		public PropertyMetaClassArray (Modelizer context, FieldInfo fieldInfo) : base (context, fieldInfo)
		{
			elementType = fieldInfo.FieldType.GetElementType ();
		}

		public override void SetValue (System.Object target, System.Object value)
		{
			var newValue = (JsonArray)value;
			var targetValue = (IList)Activator.CreateInstance (fieldInfo.FieldType, newValue.Count);

			for (int i = 0; i < newValue.Count; i++) {
				targetValue [i] = context.Modelize (elementType, null, newValue [i]);
			}

			fieldInfo.SetValue (target, targetValue);
		}
	}
}
