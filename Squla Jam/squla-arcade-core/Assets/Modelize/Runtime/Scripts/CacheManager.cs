using System;
using System.Collections.Generic;
using SimpleJson;
using Squla.Core;
using Squla.Core.ObjectPooling;

namespace Squla.Core.Modelize
{
	public class CacheManager
	{
		private Dictionary<string, IPoolableObject> cache = new Dictionary<string, IPoolableObject> ();
		private CacheKey keyResolver;
		private CachePoolManager poolManager;

		public CacheManager (Type targetType, string keyField, int poolSize)
		{
			keyResolver = new CacheKey (keyField);
			poolManager = new CachePoolManager (targetType, poolSize);
		}

		public IPoolableObject Resolve (JsonObject json)
		{
			var key = keyResolver.Resolve (json);
			IPoolableObject target;
			if (cache.ContainsKey (key)) {
				target = cache [key];
			} else {
				cache [key] = target = poolManager.Resolve ();
			}

			return target;
		}

		public void Flush ()
		{
			// go over cache and release objects to pool
			foreach (var pair in cache) {
				poolManager.Release (pair.Value);
			}

			// and clear cache.
			cache.Clear ();
		}

		class CachePoolManager : PoolManager<IPoolableObject>
		{
			private Type targetType;

			public CachePoolManager (Type targetType, int poolSize = 50) : base (poolSize)
			{
				this.targetType = targetType;
			}

			protected override IPoolableObject CreateInstance ()
			{
				return (IPoolableObject)Activator.CreateInstance (targetType);
			}
		}
	}
}

