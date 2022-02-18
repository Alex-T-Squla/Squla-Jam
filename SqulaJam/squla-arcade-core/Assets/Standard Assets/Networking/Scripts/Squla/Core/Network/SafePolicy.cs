using UnityEngine;
using System.Collections;

namespace Squla.Core.Network
{
	public class SafePolicy : IResponseCodePolicy
	{
		public bool CanHandle (int code)
		{
			return (code == 200 || code == 400);
		}
	}
}

