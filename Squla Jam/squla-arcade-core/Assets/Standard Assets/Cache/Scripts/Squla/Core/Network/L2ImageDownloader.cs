using UnityEngine;
using System;
using System.Collections;
using System.IO;
using Squla.Core.ZeroQ;

namespace Squla.Core.Network
{
	public class L2ImageDownloader : IFallbackImageDownloader
	{
		private readonly QueuedExecutor taskExecutor;
		private readonly IFallbackImageDownloader fallback;

		private readonly LRUFileSystem<Sprite> cache;

		private readonly Bus bus;
		
		public L2ImageDownloader (int cacheSize, string path, ICoroutineManager manager, IFallbackImageDownloader fallback, Bus bus)
		{
			this.fallback = fallback;
			cache = new LRUFileSystem<Sprite> (cacheSize, path);
			taskExecutor = new QueuedExecutor (manager);
			this.bus = bus;
		}

		public void GetImage (string url, Action<string, Sprite> OnFinished)
		{
			if (cache.HasItem (url)) {
				Func<IEnumerator> action = () => {

					// reading from cache is added into the queue.
					// this ensures that the item is not removed by cache eviction.
					if (cache.HasItem (url)) {
						return cache.LoadSpriteFromFile (url, OnFinished);
					}

					GetImage (url, OnFinished);
					return null;
				};
				taskExecutor.AddToQueue (action);
				return;
			}

			if (fallback == null)
				return;

			Action<string, Sprite> finished = (str, sprite) => {
				OnSpriteReady (str, sprite);
				OnFinished (str, sprite);
			};
			fallback.GetImage (url, finished);
		}

		private void OnSpriteReady (string url, Sprite sprite)
		{
			if (sprite == null) return;
			try {
				cache.Insert(url, sprite);
			} catch (IOException) {
				bus.Publish("cmd://the-app/storage-warning");
			}
		}

		public void Flush ()
		{
			taskExecutor.Flush ();
			fallback.Flush ();
		}
	}
}
