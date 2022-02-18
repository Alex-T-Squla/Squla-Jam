using System;

namespace Squla.Core.ZeroQ
{
	[AttributeUsage (AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
	public class Subscribe : Attribute
	{
		private string _name;

		public string Name {
			get { return _name; }
		}

		public Subscribe (string name)
		{
			_name = name;
		}
	}
}
