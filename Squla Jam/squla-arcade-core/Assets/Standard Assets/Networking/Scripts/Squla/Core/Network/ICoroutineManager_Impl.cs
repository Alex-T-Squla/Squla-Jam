using System.Collections;
using UnityEngine;

namespace Squla.Core.Network
{
	public class ICoroutineManager_Impl : MonoBehaviour, ICoroutineManager
	{
		public Coroutine CreateCoroutine (IEnumerator task)
		{
			return StartCoroutine (task);
		}

		public void StopRoutine (Coroutine routine)
		{
			StopCoroutine (routine);
		}

		public void ExecuteInNextFrame(System.Action callback)
		{
			StartCoroutine(ExecuteInNextFrame_Impl(callback));
		}

		private IEnumerator ExecuteInNextFrame_Impl(System.Action callback)
		{
			yield return null;
			callback();
		}
	}
}
