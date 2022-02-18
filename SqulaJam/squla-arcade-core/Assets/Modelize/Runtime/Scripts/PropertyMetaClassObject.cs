using System;
using System.Reflection;
using SimpleJson;

namespace Squla.Core.Modelize
{
	public class PropertyMetaClassObject: PropertyMetaClass
	{
		public PropertyMetaClassObject (Modelizer context, FieldInfo fieldInfo) : base (context, fieldInfo)
		{
		}

		public override void SetValue (System.Object target, System.Object value)
		{
			System.Object newValue = context.Modelize (fieldInfo.FieldType, fieldInfo.GetValue (target), value);
			fieldInfo.SetValue (target, newValue);
		}
	}
}

