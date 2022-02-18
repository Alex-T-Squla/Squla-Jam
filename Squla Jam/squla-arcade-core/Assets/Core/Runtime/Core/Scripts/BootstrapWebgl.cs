using System.Collections;
using Squla.Core.IOC;
using Squla.Core.IOC.Builder;
using Squla.Core.ZeroQ;
using UnityEngine;
using Object = System.Object;

namespace Squla.TheApp
{
	[SingletonModule]
	public class BootstrapWebgl : MonoBehaviour, IBusDelegate, IPrefabProvider
	{
		public ModuleModelSet startupModules;
		public AudioListener audioListener;
#if UNITY_EDITOR
		public Bus Bus => bus;
#endif

		public Transform startupTarget;
		private ObjectGraph graph;
		private Bus bus;
		private WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();

		[Provides]
		[Singleton]
		public Bus provideBus()
		{
			return bus;
		}

		[Provides]
		[Singleton]
		public IPrefabProvider providePrefabProvider()
		{
			return this;
		}

		private void Awake()
		{
			ObjectGraph.main = graph = ObjectGraph.Create(this);
			bus = new Bus(this);
			StartCoroutine(LoadStartupModules());
		}

		private IEnumerator LoadStartupModules()
		{
			var dataSet = startupModules.dataSet;

			for (int i = 0; i < dataSet.Length; i++)
			{
				// wait for end of frame to allow the full initialization of module.
				yield return endOfFrame;

				var moduleInfo = dataSet[i];
				if (!moduleInfo.enabled)
					continue;

				Debug.Log($"---load {moduleInfo.name}");
				Instantiate(moduleInfo.prefab, startupTarget, false);
			}
		}

		public void DelayedPublish(string command, object source)
		{
			StartCoroutine(DelayedPublishWorker(command, source));
		}

		public void DelayedRegister(Object target)
		{
			StartCoroutine(DelayedRegisterWorker(target));
		}

		public void Notify(string command, object source)
		{
			//Not implemented
		}

		private IEnumerator DelayedPublishWorker(string command, Object source)
		{
			yield return endOfFrame;

			bus.Publish(command, source);
		}

		private IEnumerator DelayedRegisterWorker(Object target)
		{
			yield return endOfFrame;

			bus.Register(target);
		}

		public GameObject this[string name] => null;
	}
}