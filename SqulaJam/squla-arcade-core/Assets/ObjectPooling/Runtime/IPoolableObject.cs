using System;

namespace Squla.Core.ObjectPooling
{
	public interface IPoolableObject
	{
		void Release ();
	}
}

