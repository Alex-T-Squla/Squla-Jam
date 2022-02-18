using System;

namespace Squla.Core.IOC
{
	[AttributeUsage (AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class Provides : Attribute
	{
		public string Name;
	}
}

