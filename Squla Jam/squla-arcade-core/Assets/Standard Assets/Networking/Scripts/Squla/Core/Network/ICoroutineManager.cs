using System.Collections;
using UnityEngine;

namespace Squla.Core.Network
{
	public interface ICoroutineManager
	{
		Coroutine CreateCoroutine (IEnumerator task);

		void StopCoroutine (Coroutine routine);

		void ExecuteInNextFrame(System.Action callback);
	}
}
