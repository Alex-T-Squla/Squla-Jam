using System;
using System.Reflection;

namespace Squla.Core.Modelize
{
	public abstract class PropertyMetaClass
	{
		protected Modelizer context;
		protected FieldInfo fieldInfo;

		public string FieldName { 
			get { return fieldInfo.Name; }
		}

		public PropertyMetaClass (Modelizer context, FieldInfo fieldInfo)
		{
			this.context = context;
			this.fieldInfo = fieldInfo;
		}

		public abstract void SetValue (System.Object target, System.Object value);
	}
}
