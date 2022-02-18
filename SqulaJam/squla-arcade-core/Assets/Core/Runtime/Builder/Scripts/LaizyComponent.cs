using System;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Squla.Core.IOC.Builder
{
	[Serializable]
	public class LaizyComponent
	{
		[SerializeField] private string prefabName;
		[SerializeField] private GameObject prefab;
		[SerializeField] private RectTransform target;
		
		
		[Tooltip("If this is true the component will activate or deactivate the target.GameObject instead of the instantiated Prefab")]
		[SerializeField]
		private bool disableTargetInsteadChild;

		[Header("Assign if this laizy component already exists")]
		[SerializeField]
		private GameObject instance;

		public RectTransform Target => target;

		private T Get<T> (bool active, IPrefabProvider prefabProvider = null) where T: class
		{
			if (target == null) {
				throw new IOCException ("LaizyComponent target is null");
			}

			if (!active) {
				if (disableTargetInsteadChild) {
					target.gameObject.SetActive(false);
				} else if (instance != null){
					instance.SetActive(false);
				}
				return null;
			}

			if (instance == null) {
				var p = prefab;

				if (!p && prefabProvider != null) {
					p = prefabProvider[prefabName];
				}
				if (!p) {
					Debug.LogError($"LaizyComponent prefab is null, if you're using segmentation make sure to also include the prefabProvider");
					return null;
				}
				instance = Object.Instantiate (p);
				var t = instance.GetComponent<RectTransform> ();
				t.SetParent (target, false);
			}

			if (disableTargetInsteadChild) {
				target.gameObject.SetActive(true);
			} else {
				instance.SetActive(true);
			}
			return instance.GetComponent<T> ();
		}

		public bool Get<T>(bool active, out T element) where T: class
		{
			element = Get<T>(active);
			return active;
		}
		
		public bool Get<T>(bool active, IPrefabProvider prefabProvider, out T element) where T: class
		{
			element = Get<T>(active, prefabProvider);
			return active;
		}

		public void SetPrefab(GameObject prefab)
		{
			Unload();
			this.prefab = prefab;
		}

		public void Unload()
		{
			if (instance) {
				Object.Destroy(instance);
				instance = null;
			}
		}
		public GameObject Instance => instance;
		public GameObject Prefab => prefab;

		public GameObject GetPrefab(IPrefabProvider prefabProvider)
		{
			if (prefab) return prefab;
			if (prefabName != "") return prefabProvider[prefabName];
			return null;
		}
	}
}
