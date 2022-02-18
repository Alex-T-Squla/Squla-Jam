using UnityEngine;
using System.Collections;

namespace Squla.Core.Network
{
	public class SafestPolicy : IResponseCodePolicy
	{
		public bool CanHandle (int code)
		{
			return code == 200;
		}
	}
}

