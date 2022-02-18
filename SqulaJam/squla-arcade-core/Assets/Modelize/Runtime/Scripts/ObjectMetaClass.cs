using System;
using System.Collections.Generic;
using Squla.Core;
using System.Reflection;
using SimpleJson;
using Squla.Core.ObjectPooling;
using UnityEngine;

namespace Squla.Core.Modelize
{
	public abstract class ObjectMetaClass : TypeMetaClass
	{
		private static PropertyMetaClass[] buffer = new PropertyMetaClass[100];

		protected PropertyMetaClass[] fields;

		protected ObjectMetaClass (Modelizer context, Type targetType) : base (targetType)
		{
			var fieldInfos = targetType.GetFields (BindingFlags.Public | BindingFlags.Instance);

			int index = 0;
			for (int i = 0; i < fieldInfos.Length; i++) {
				var fieldInfo = fieldInfos [i];

				if (Attribute.GetCustomAttribute (fieldInfo, typeof(NonSerializedAttribute)) != null)
					continue;

				var fieldType = fieldInfo.FieldType;
				var fieldTypeName = fieldType.FullName;

				bool isGenericList = fieldType.IsGenericType && fieldType.GetGenericTypeDefinition () == typeof(List<>);
				bool isGenericDict = fieldType.IsGenericType && fieldType.GetGenericTypeDefinition () == typeof(Dictionary<,>);
				if (isGenericList) {
					buffer [index++] = new PropertyMetaClassList (context, fieldInfo);
				} else if (isGenericDict) {
					buffer [index++] = new PropertyMetaClassDictionary (context, fieldInfo);
				} else if (fieldType.IsArray) {
					buffer [index++] = new PropertyMetaClassArray (context, fieldInfo);
				} else {
					buffer [index++] = new PropertyMetaClassObject (context, fieldInfo);
				}
			}

			fields = new PropertyMetaClass[index];
			for (int i = 0; i < index; i++) {
				fields [i] = buffer [i];
			}
		}

		public override System.Object Modelize (System.Object target, System.Object source)
		{
			if (!(source is JsonObject)) {
				Debug.LogError($"{source} is not a jsonObject, target is {target}");
			}
			var jsonSource = (JsonObject)source;

			if (target == null)
				target = Resolve (jsonSource);

			for (int i = 0; i < fields.Length; i++) {
				var field = fields [i];
				if (jsonSource.ContainsKey (field.FieldName)) {
					field.SetValue (target, jsonSource [field.FieldName]);
				}
			}

			return target;
		}

		protected abstract System.Object Resolve (JsonObject source);

		public static ObjectMetaClass Create (Modelizer context, Type targetType)
		{
			var implementsPoolable = typeof(IPoolableObject).IsAssignableFrom (targetType);

			var attr = (ModelizeAttribute)Attribute.GetCustomAttribute (targetType, typeof(ModelizeAttribute));
			if (attr != null && !implementsPoolable) {
				throw new Exception ("[Modelize] able class should implement IPoolableObject interface");
			}

			if (attr != null) {
				return new ObjectMetaClassModelizable (context, targetType, attr);
			} else {
				return new ObjectMetaClassDefault (context, targetType);
			}
		}
	}
}

