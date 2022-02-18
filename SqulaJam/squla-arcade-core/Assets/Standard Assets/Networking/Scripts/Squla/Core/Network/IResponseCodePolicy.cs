using UnityEngine;
using System.Collections;

namespace Squla.Core.Network
{
	public interface IResponseCodePolicy
	{
		bool CanHandle (int code);
	}
}
