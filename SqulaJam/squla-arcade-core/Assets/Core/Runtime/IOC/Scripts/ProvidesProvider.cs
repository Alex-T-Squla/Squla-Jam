using System;
using System.Reflection;


namespace Squla.Core.IOC
{
	internal class ProvidesProvider : Provider
	{
		private Type targetType;
		private System.Object target;
		private MethodInfo methodInfo;

		public ProvidesProvider (ObjectGraph graph, Type targetType, System.Object target, MethodInfo methodInfo, bool singleton) : base (graph, singleton)
		{
			this.targetType = targetType;
			this.target = target;
			this.methodInfo = methodInfo;
			this.dependentNamedTypes = graph.GetParamtersNamedType (methodInfo.GetParameters ());
		}

		protected override System.Object CreateInstance ()
		{
			if (target == null) {
				// if target is null resolve it first
				target = graph.Get (targetType);
			}

			// for the all the method arguments
			// resolve dependencies and pass as arguments.
			var arguments = graph.Resolve (dependentNamedTypes);

			return methodInfo.Invoke (target, arguments);
		}
	}
}

