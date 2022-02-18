using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Squla.Core.IOC.Builder
{
	public class PoolBuilder<T> where T : Component
	{
		[SerializeField]
		private GameObject prefab;
		[SerializeField]
		private Transform target;

		private Queue<T> pool;

		private Action<T> resetObject;

		public PoolBuilder(GameObject prefab, Transform target, int capacity, Action<T> resetObject = null)
		{
			this.prefab = prefab;
			this.target = target;
			this.resetObject = resetObject;
			pool =new Queue<T>(capacity);
		}

		public void Release(T pooledObject)
		{
			pooledObject.gameObject.SetActive(false);
			pool.Enqueue(pooledObject);
		}

		public T Get()
		{
			T pooledObject;
			if (pool.Count > 0) {
				pooledObject = pool.Dequeue();
				if (resetObject != null) {
					resetObject(pooledObject);
				}
				pooledObject.gameObject.SetActive(true);
			} else {
				pooledObject = Object.Instantiate(prefab, target, false).GetComponent<T>();
			}
			return pooledObject;
		}
	}
}
