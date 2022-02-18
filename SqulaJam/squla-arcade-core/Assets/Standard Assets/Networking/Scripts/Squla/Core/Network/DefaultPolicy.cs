using UnityEngine;
using System.Collections;

namespace Squla.Core.Network
{
	public class DefaultPolicy : IResponseCodePolicy
	{
		public bool CanHandle (int code)
		{
			return true;
		}
	}
}

