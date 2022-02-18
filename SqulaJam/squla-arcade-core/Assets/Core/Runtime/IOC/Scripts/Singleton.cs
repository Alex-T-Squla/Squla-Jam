using System;

namespace Squla.Core.IOC
{
	[AttributeUsage (AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class Singleton: Attribute
	{
		public string ContextName {
			get;
			private set;
		}

		public Singleton ()
		{
		}

		public Singleton (string contextName)
		{
			ContextName = contextName;
		}
	}
}

