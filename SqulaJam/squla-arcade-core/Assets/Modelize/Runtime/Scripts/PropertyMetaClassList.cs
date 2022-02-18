using System;
using System.Collections;
using System.Reflection;
using SimpleJson;
using UnityEngine;

namespace Squla.Core.Modelize
{
	public class PropertyMetaClassList:PropertyMetaClass
	{
		private Type elementType;

		public PropertyMetaClassList (Modelizer context, FieldInfo fieldInfo) : base (context, fieldInfo)
		{
			elementType = fieldInfo.FieldType.GetGenericArguments () [0];
		}

		public override void SetValue (System.Object target, System.Object value)
		{
			var newValue = (JsonArray)value;
			var targetValue = (IList)fieldInfo.GetValue (target);

			if (targetValue == null) {
				Debug.LogError($"Don't forget to initialize your list in {target}");
			}
			targetValue.Clear ();

			for (int i = 0; i < newValue.Count; i++) {
				targetValue.Add (context.Modelize (elementType, null, newValue [i]));
			}
		}
	}
}

