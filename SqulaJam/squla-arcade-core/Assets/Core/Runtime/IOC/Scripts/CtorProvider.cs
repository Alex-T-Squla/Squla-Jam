using System;
using System.Reflection;
using System.Linq;


namespace Squla.Core.IOC
{
	internal class CtorProvider : Provider
	{
		private ConstructorInfo ctorInfo;
		private Type targetType;

		public CtorProvider (ObjectGraph graph, Type targetType) : base (graph, false)
		{
			this.targetType = targetType;

			Singleton singletonAttr = (Singleton)Attribute.GetCustomAttribute (targetType, typeof(Singleton));
			if (singletonAttr != null) {
				isSingleton = true;
			}

			SingletonModule singletonModuleAttr = (SingletonModule)Attribute.GetCustomAttribute (targetType, typeof(SingletonModule));
			if (singletonModuleAttr != null) {
				isSingleton = true;
			}

			var ctorInfos = targetType.GetConstructors (BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public);

			ConstructorInfo ctorInfo = null;

			for (int i = 0; i < ctorInfos.Length; i++) {
				var info = ctorInfos [i];

				var attr = (Inject)Attribute.GetCustomAttribute (info, typeof(Inject));
				if (attr != null) {
					ctorInfo = info;
					break;
				}
			}

			this.ctorInfo = ctorInfo;
			if (ctorInfo != null) {
				this.dependentNamedTypes = graph.GetParamtersNamedType (ctorInfo.GetParameters ());
			}
		}

		protected override System.Object CreateInstance ()
		{
			if (ctorInfo == null) {
				throw new IOCException (string.Format ("No [Inject] 'able constructor found for {0}", targetType.FullName));
			}

			// for the all the method arguments
			// resolve dependencies and pass as arguments.
			var arguments = graph.Resolve (dependentNamedTypes);

			// create object
			var target = ctorInfo.Invoke (arguments);

			// inject their fields
			graph.InjectFields (target);

			return target;
		}
	}
}

