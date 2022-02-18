using System;

namespace Squla.Core.IOC
{
	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Constructor | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
	public class Inject : Attribute
	{
		public string Name {
			get ;
			private set;
		}

		public Inject ()
		{
		}

		public Inject (string name)
		{
			Name = name;
		}
	}
}

