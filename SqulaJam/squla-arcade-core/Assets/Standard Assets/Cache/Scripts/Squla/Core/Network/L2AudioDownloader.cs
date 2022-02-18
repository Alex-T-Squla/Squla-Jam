using UnityEngine;
using System;
using System.Collections;
using System.IO;
using Squla.Core.ZeroQ;

namespace Squla.Core.Network
{
	public class L2AudioDownloader : IFallbackAudioDownloader
	{
		private readonly QueuedExecutor taskExecutor;
		private readonly IFallbackAudioDownloader fallback;

		private readonly LRUFileSystem<AudioClip> cache;

		private readonly Bus bus;
		
		public L2AudioDownloader (int cacheSize, string path, ICoroutineManager manager, IFallbackAudioDownloader fallback, Bus bus)
		{
			this.fallback = fallback;
			cache = new LRUFileSystem<AudioClip> (cacheSize, path);
			taskExecutor = new QueuedExecutor (manager);
			this.bus = bus;
		}

		public void GetAudioClip (string url, Action<string, AudioClip, byte[]> OnFinished)
		{
			if (cache.HasItem (url)) {
				Func<IEnumerator> action = () => {

					// reading from cache is added into the queue.
					// this ensures that the item is not removed by cache eviction.
					if (cache.HasItem (url)) {
						return cache.LoadClipFromFile (url, OnFinished);
					}

					GetAudioClip (url, OnFinished);
					return null;
				};
				taskExecutor.AddToQueue (action);
				return;
			}

			if (fallback == null)
				return;

			Action<string, AudioClip, byte[]> finished = (str, ac, b) => {
				OnAudioClipReady (str, ac, b);
				OnFinished (str, ac, b);
			};
			fallback.GetAudioClip (url, finished);
		}

		private void OnAudioClipReady (string url, AudioClip audioClip, byte[] bytes)
		{
			if (audioClip != null) {
				//hack - this doesn't actually write audio clip to file
				cache.Insert (url, audioClip);
				//this does
				try{
					cache.SaveBytesToFile (bytes, url);
				} catch (IOException e) {
					Debug.LogWarning(e.Message);
					bus.Publish("cmd://the-app/storage-warning");
				}
			}
		}
	}
}
