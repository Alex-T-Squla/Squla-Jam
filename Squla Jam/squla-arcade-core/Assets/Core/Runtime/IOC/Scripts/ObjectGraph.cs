using System;
using System.Collections.Generic;
using System.Reflection;

namespace Squla.Core.IOC
{
	public sealed class ObjectGraph
	{
		public static ObjectGraph main;

		private readonly Dictionary<string, Provider> providers = new Dictionary<string, Provider> ();

		// Each Type that has [Inject] annotated on their fields, has one instance here.
		private readonly Dictionary<string, FieldsInjector> injectors = new Dictionary<string, FieldsInjector> ();

		private ObjectGraph ()
		{
		}

		public void RegisterModules (params Type[] moduleTypes)
		{
			for (int i = 0; i < moduleTypes.Length; i++) {
				RegisterModule (moduleTypes [i], null);
			}
		}

		public void RegisterModule (System.Object mainModule)
		{
			RegisterModule (mainModule.GetType (), mainModule);
		}

		public void UnRegisterModule (System.Object mainModule)
		{
			throw new NotImplementedException ("Yet to be implemented");
		}

		private void RegisterModule (Type mainModuleType, System.Object mainModule)
		{
			// if Type contains Module attribute, then it contains [Provides] attributes

			// if Type is [SingletonModule] then only one instance.

			var attr = Attribute.GetCustomAttribute (mainModuleType, typeof(SingletonModule));
			if (attr == null) {
				throw new IOCException (string.Format ("In order to be a Module {0} should have [SingletonModule] attribute", mainModuleType.FullName));
			}

			var methodInfos = mainModuleType.GetMethods (BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
			for (int i = 0; i < methodInfos.Length; i++) {
				var mInfo = methodInfos [i];

				Provides providesAttr = (Provides)Attribute.GetCustomAttribute (mInfo, typeof(Provides));

				Singleton singletonAttr = (Singleton)Attribute.GetCustomAttribute (mInfo, typeof(Singleton));
				bool singleton = singletonAttr != null;

				if (providesAttr != null) {

					var key = NamedType (providesAttr.Name, mInfo.ReturnType);
					var value = new ProvidesProvider (this, mainModuleType, mainModule, mInfo, singleton);

					// if provider already found, override it and give warning about it.
					if (providers.ContainsKey (key)) {
						// overriding
					}
					providers [key] = value;
				}
			}
		}

		public static ObjectGraph Create (System.Object mainModule)
		{
			var graph = new ObjectGraph ();
			graph.RegisterModule (mainModule);
			return graph;
		}

		public void Resolve (System.Object target)
		{
			InjectFields (target);
		}

		public T Get<T> ()
		{
			return Get<T> (null);
		}

		public T Get<T> (string name)
		{
			var key = NamedType (name, typeof(T));
			return (T)Get (key, typeof(T));
		}

		public T GetOrDefault<T> (string name)
		{
			var key = NamedType (name, typeof(T));
			if (providers.ContainsKey (key)) {
				var resp = providers [key].Instance ();
				return (T)resp;
			}

			return default(T);
		}

		public System.Object Get (Type targetType)
		{
			var key = NamedType (null, targetType);
			return Get (key, targetType);
		}

		private System.Object Get (string name, Type targetType)
		{
			if (targetType.IsInterface && !providers.ContainsKey (name)) {
				// if targetType is interface, provider should exist.
				// otherwise throw exception
				throw new IOCException (string.Format ("{0} requested implementation, but no provider exists.", name));
			}

			// check providers for the type exists.  Here targetType is a class type.
			if (!providers.ContainsKey (name)) {
				providers.Add (name, new CtorProvider (this, targetType));
			}

			return providers [name].Instance ();
		}

		internal void InjectFields (System.Object target)
		{
			var mainType = target.GetType ();

			// Each type that needs injection, we have instance of type FieldsInjector that is build once,
			// with all the information about the fields to be injected.
			// Afterwards the built information is used to resolve the dependencies.
			if (!injectors.ContainsKey (mainType.FullName)) {

				// if no FieldsInjector for the given type, construct one.
				// If for some reason, we come to a conclusion, this construction slows down or affects the performance
				// we go for compile time solution of generating the class for each type,
				// that does the work.
				injectors.Add (mainType.FullName, new FieldsInjector (this, mainType));
			}

			var injector = injectors [mainType.FullName];
			injector.Resolve (target);
		}

		internal System.Object[] Resolve (string[] namedTypes)
		{
			System.Object[] objects = new System.Object[namedTypes.Length];

			for (int i = 0; i < namedTypes.Length; i++) {
				var namedType = namedTypes [i];
				objects [i] = providers [namedType].Instance ();
			}

			return objects;
		}

		internal string NamedType (string name, Type type)
		{
			return string.Format ("/{0}/{1}", name, type.FullName);
		}

		internal string[] GetParamtersNamedType (ParameterInfo[] parameterInfos)
		{
			string[] namedTypes = new string[parameterInfos.Length];

			for (int i = 0; i < parameterInfos.Length; i++) {
				var pInfo = parameterInfos [i];

				Inject attr = (Inject)Attribute.GetCustomAttribute (pInfo, typeof(Inject));

				string name = (attr != null) ? attr.Name : null;
				var namedType = NamedType (name, pInfo.ParameterType);

				Ensure (namedType, pInfo.ParameterType);

				namedTypes [i] = namedType;
			}

			return namedTypes;
		}

		internal void Ensure (string namedType, Type targetType)
		{
			if (!providers.ContainsKey (namedType)) {
				providers.Add (namedType, new CtorProvider (this, targetType));
			}
		}

		internal void RegisterDataProvider (DataProviderItem dataSet)
		{
			var targetType = dataSet.target.GetType ();

			var namedType = NamedType (dataSet.name, targetType);
			if (!providers.ContainsKey (namedType)) {
				providers.Add (namedType, new DataProvider (this, targetType, dataSet.target));
			}
		}

		internal void UnRegisterDataProvider (DataProviderItem dataSet)
		{
			var targetType = dataSet.target.GetType ();

			var namedType = NamedType (dataSet.name, targetType);
			if (providers.ContainsKey (namedType)) {
				providers.Remove (namedType);
			}
		}

		internal void RegisterRectTransformProvider (RectTransformItem dataSet)
		{
			var targetType = dataSet.target.GetType ();

			var namedType = NamedType (dataSet.name, targetType);
			if (!providers.ContainsKey (namedType)) {
				providers.Add (namedType, new RectTransformProvider (this, targetType, dataSet.target));
			}
		}

		internal void UnRegisterRectTransformProvider (RectTransformItem dataSet)
		{
			var targetType = dataSet.target.GetType ();

			var namedType = NamedType (dataSet.name, targetType);
			if (providers.ContainsKey (namedType)) {
				providers.Remove (namedType);
			}
		}
	}

}

