using System;

namespace Squla.Core.IOC
{
	[AttributeUsage (AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class SingletonModule: Attribute
	{
	}
}

