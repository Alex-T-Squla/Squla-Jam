using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Squla.Core.IOC.Builder;


namespace Squla.Core.IOC
{
	internal class FieldsInjector
	{
		protected ObjectGraph graph;

		// Null pattern is used to optimize the memory usage.
		private FieldMeta fieldMeta = FieldMeta.Null;
		private ComponentFieldMeta componentMeta = ComponentFieldMeta.Null;
		private LazyFieldMeta lazyMeta = LazyFieldMeta.Null;

		public FieldsInjector (ObjectGraph graph, Type mainType)
		{
			this.graph = graph;

			var fieldInfos = mainType.GetFields (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
			for (int i = 0; i < fieldInfos.Length; i++) {
				var fieldInfo = fieldInfos [i];

				var attr1 = (Inject)Attribute.GetCustomAttribute (fieldInfo, typeof(Inject));
				if (attr1 != null) {
					if (fieldInfo.FieldType.IsGenericType) {
						// check for Lazy
						// fieldInfo.FieldType.FullName starts with Squla.Core.IOC.Laizy`
						var underlyingType = fieldInfo.FieldType.GetGenericArguments () [0];
						Init (lazyMeta);
						lazyMeta.Prepare (attr1.Name, fieldInfo, underlyingType);
					} else {
						Init (fieldMeta);
						fieldMeta.Prepare (attr1.Name, fieldInfo);
					}
					continue;
				}

				var attr2 = (InjectComponent)Attribute.GetCustomAttribute (fieldInfo, typeof(InjectComponent));
				if (attr2 != null) {
					// Track these fields in another array.
					// from the given target object find the Builder component,
					// this component has named items in it which it constructed from prefab.
					// if we ask by name from [InjectComponent("name")] and get the type of the component.
					Init (componentMeta);
					componentMeta.Prepare (attr2.Name, fieldInfo);
					continue;
				}
			}
		}

		private void Init (FieldMeta meta)
		{
			if (meta != FieldMeta.Null)
				return;

			fieldMeta = new FieldMeta ();
			fieldMeta.Init (graph);
		}

		private void Init (ComponentFieldMeta meta)
		{
			if (meta != ComponentFieldMeta.Null)
				return;

			componentMeta = new ComponentFieldMeta ();
			componentMeta.Init (graph);
		}

		private void Init (LazyFieldMeta meta)
		{
			if (meta != LazyFieldMeta.Null)
				return;

			lazyMeta = new LazyFieldMeta ();
			lazyMeta.Init (graph);
		}

		public void Resolve (System.Object target)
		{
			fieldMeta.Resolve (target);
			componentMeta.Resolve (target);
			lazyMeta.Resolve (target);
		}

		abstract class AbstractMeta
		{
			protected ObjectGraph graph;
			protected List<FieldInfo> targetFieldInfos = new List<FieldInfo> ();
			protected List<string> targetNamedTypes = new List<string> ();

			public void Init (ObjectGraph graph)
			{
				this.graph = graph;
			}

			public abstract void Resolve (System.Object target);
		}

		class FieldMeta : AbstractMeta
		{
			public static FieldMeta Null = new FieldMeta ();

			public void Prepare (string name, FieldInfo fieldInfo)
			{
				var namedType = graph.NamedType (name, fieldInfo.FieldType);
				targetNamedTypes.Add (namedType);
				targetFieldInfos.Add (fieldInfo);
				graph.Ensure (namedType, fieldInfo.FieldType);
			}

			public override void Resolve (System.Object target)
			{
				if (targetNamedTypes.Count == 0)
					return;
				
				var objects = graph.Resolve (targetNamedTypes.ToArray ());

				for (int i = 0; i < targetNamedTypes.Count; i++) {
					var fieldInfo = targetFieldInfos [i];
					fieldInfo.SetValue (target, objects [i]);
				}
			}
		}

		class ComponentFieldMeta : AbstractMeta
		{
			public static ComponentFieldMeta Null = new ComponentFieldMeta ();

			private List<string> shortNames = new List<string> ();

			public void Prepare (string name, FieldInfo fieldInfo)
			{
				var namedType = graph.NamedType (name, fieldInfo.FieldType);
				targetFieldInfos.Add (fieldInfo);
				shortNames.Add (name);
				targetNamedTypes.Add (namedType);
			}

			public override void Resolve (System.Object target)
			{
				if (targetNamedTypes.Count == 0)
					return;
				
				var targetComponent = (Component)target;
				var simpleBuilder = targetComponent.gameObject.GetComponent<SimpleBuilder> ();
				var listBuilder = targetComponent.gameObject.GetComponent<ListBuilder> ();

				if (simpleBuilder == null && listBuilder == null) {
					throw new IOCException (string.Format ("Trying to resolve, but no SimpleBuilder or ListBuilder attached to the '{0}'", targetComponent.gameObject.name));
				}

				for (int i = 0; i < targetNamedTypes.Count; i++) {
					var name = shortNames [i];
					var fieldInfo = targetFieldInfos [i];

					if (listBuilder != null && listBuilder._name == name) {
						fieldInfo.SetValue (target, listBuilder);
					} else {
						Component obj = simpleBuilder.Resolve (name, fieldInfo.FieldType);
						fieldInfo.SetValue (target, obj);
					}
				}
			}
		}

		class LazyFieldMeta : AbstractMeta
		{
			public static LazyFieldMeta Null = new LazyFieldMeta ();

			private List<ConstructorInfo> underlyingTypes = new List<ConstructorInfo> ();

			public void Prepare (string name, FieldInfo fieldInfo, Type underlyingType)
			{
				targetNamedTypes.Add (name);
				targetFieldInfos.Add (fieldInfo);

				var infos = fieldInfo.FieldType.GetConstructors (BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic);
				var ctorInfo = infos [0];
				underlyingTypes.Add (ctorInfo);

				var namedType = graph.NamedType (name, underlyingType);
				graph.Ensure (namedType, underlyingType);
			}

			public override void Resolve (System.Object target)
			{
				for (int i = 0; i < targetNamedTypes.Count; i++) {
					var name = targetNamedTypes [i];
					var ctorInfo = underlyingTypes [i];
					var fieldInfo = targetFieldInfos [i];

					var instance = ctorInfo.Invoke (new string[] { name });
					fieldInfo.SetValue (target, instance);
				}
			}
		}
	}

}
