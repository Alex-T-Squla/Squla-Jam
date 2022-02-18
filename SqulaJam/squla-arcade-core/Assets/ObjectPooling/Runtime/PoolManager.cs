using System;
using System.Collections.Generic;
using Squla.Core.Logging;

namespace Squla.Core.ObjectPooling
{
	public class PoolManager<T> where T : IPoolableObject
	{
		private T[] pool;
		private int index;
		private SmartLogger logger;

		public PoolManager (int poolSize = 50)
		{
			logger = SmartLogger.GetLogger (this.GetType ().ToString ());
			logger.Debug ("Constructor of PoolManager for type: {0}", this.GetType ().ToString ());
			pool = new T[poolSize];
			index = 0;
		}

		public T Resolve ()
		{
			if (index > 0) {
				return pool [--index];
			} else {
				return CreateInstance ();
			}
		}

		public void Release (T target)
		{
			target.Release ();

			if (index >= pool.Length) {
				logger.Info (String.Format ("Index ({0}) >= pool size ({1})", index, pool.Length));
				return;
			}

			pool [index++] = target;
		}

		protected virtual T CreateInstance ()
		{
			return Activator.CreateInstance<T> ();
		}
	}
}

