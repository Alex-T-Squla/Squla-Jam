using UnityEngine;
using System.Collections.Generic;
using System;
using Squla.Core.Logging;

namespace Squla.Core.Network
{
	public class L1ImageDownloader : IImageDownloader
	{
		private readonly SmartLogger logger = SmartLogger.GetLogger<L1ImageDownloader> ();

		private readonly Dictionary<string, Action<string, Sprite>> runningJobs = new Dictionary<string, Action<string, Sprite>> ();

		private readonly LRUCache<Sprite> cache;

		private readonly LRUCache<Sprite> cacheSecondary;

		private readonly IFallbackImageDownloader fallback;

		private DateTime lastUpdated;

		public L1ImageDownloader (int cacheSize, IFallbackImageDownloader fallback)
		{
			this.fallback = fallback;
			var memoryInMb = SystemInfo.systemMemorySize;
			var secondarySize = memoryInMb < 1024 ? Math.Max(cacheSize, 4) / 4 : cacheSize * 2;
			cache = new LRUCache<Sprite> (cacheSize);
			cacheSecondary = new LRUCache<Sprite> (secondarySize);
			lastUpdated = DateTime.Now;
		}

		public Sprite GetImage (string url)
		{
			return cache.GetItem (url);
		}

		public void ReleaseImage (string url)
		{
			var sprite = cache.RemoveItem (url);
			if (sprite) {
				cacheSecondary.Insert (url, sprite);
				lastUpdated = DateTime.Now;
			}
		}

		public void ReleaseImageImmediate (string url)
		{
			cache.RemoveItem (url);
		}

		public void GetImage(string url, GameObject go, Action<string, Sprite> onSprite)
		{
			if (string.IsNullOrEmpty(url))
				return;

			var sprite = cacheSecondary.RemoveItem (url);
			if (sprite) {
				cache.Insert (url, sprite);
			}
			sprite = cache.GetItem(url);
			var runningRequest = runningJobs.ContainsKey(url);
			
			Action<string, Sprite> requestComplete = (s, sprite1) => {
				if (!go || !go.activeSelf)
					return;
				onSprite(s, sprite1);
			};

			if (sprite) {
				requestComplete(url, sprite);
			} else if (runningRequest) {
				runningJobs[url] += requestComplete;
			} else {
				runningJobs [url] = requestComplete;
				fallback.GetImage (url, OnSpriteReady);
			}
		}

		private void OnSpriteReady (string url, Sprite sprite)
		{
			if (sprite != null) {
				cache.Insert (url, sprite);
			}

			if (!runningJobs.ContainsKey (url))
				return;

			var cbs = runningJobs [url];
			cbs (url, sprite);
			runningJobs.Remove (url);
		}

		public void Download (Batch batch)
		{
			for (var i = 0; i < batch.urls.Length; i++) {
				var url = batch.urls [i];
				var cacheHit = cache.GetItem(url) != null;
				var runningRequest = runningJobs.ContainsKey(url);

				if (cacheHit) {
					batch.OnSuccess ();
				} else if (runningRequest) {
					runningJobs [url] += batch.OnItemFinished;
				} else {
					runningJobs [url] = batch.OnItemFinished;
					fallback.GetImage (url, OnSpriteReady);
				}
			}
		}

		public void Flush ()
		{
			cache.Clear ();
			cacheSecondary.Clear ();
		}

		public void FlushSecondary ()
		{
			var seconds30Old = lastUpdated + TimeSpan.FromSeconds (30);
			if (seconds30Old < DateTime.Now) {
				// clear secondary only if it 30 seconds old.
				logger.Debug ("flush secondary cache {0}", cacheSecondary.Size ());
				cacheSecondary.Clear ();
			}
		}
	}
}
