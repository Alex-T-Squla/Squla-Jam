using UnityEngine;
using System.Collections.Generic;
using System;
using Squla.Core.Audio;

namespace Squla.Core.Network
{
	public class L1AudioDownloader : IAudioDownloader
	{
		private readonly Dictionary<string, Action<string, AudioClip, byte[]>> runningJobs = new Dictionary<string, Action<string, AudioClip, byte[]>> ();

		private readonly LRUCache<AudioClip> cache;

		private readonly IFallbackAudioDownloader fallback;

		private readonly IAudioclipRepository repository;

		public L1AudioDownloader (int cacheSize, IFallbackAudioDownloader fallback, IAudioclipRepository repository)
		{
			this.fallback = fallback;
			this.repository = repository;
			cache = new LRUCache<AudioClip> (cacheSize);
			cache.OnItemDeleted += Cache_OnItemDeleted;
		}

		void Cache_OnItemDeleted (string key, LRUCache<AudioClip> c)
		{
			repository.UnRegister (key);
		}

		public AudioClip GetAudioClip (string url)
		{
			return cache.GetItem (url);
		}

		public void ReleaseAudio (string url)
		{
			cache.RemoveItem (url);
		}

		public void GetAudioClip(string url, GameObject go, Action<string, AudioClip> onAudioClip)
		{
			if (string.IsNullOrEmpty(url))
				return;

			var sprite = cache.GetItem(url);
			var runningRequest = runningJobs.ContainsKey(url);

			Action<string, AudioClip, byte[]> requestComplete = (s, audioClip, b) => {
				if (!go || !go.activeSelf)
					return;
				onAudioClip(s, audioClip);
			};

			if (sprite) {
				requestComplete(url, sprite, null);
			} else if (runningRequest) {
				runningJobs[url] += requestComplete;
			} else {
				runningJobs [url] = requestComplete;
				fallback.GetAudioClip(url, OnAudioClipReady);
			}
		}

		private void OnAudioClipReady (string url, AudioClip audioClip, byte[] bytes)
		{
			if (audioClip != null) {
				cache.Insert (url, audioClip);
			}

			if (!runningJobs.ContainsKey (url))
				return;

			var cbs = runningJobs [url];
			cbs (url, audioClip, bytes);
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
					fallback.GetAudioClip (url, OnAudioClipReady);
				}
			}
		}

		public void Flush ()
		{
			cache.Clear ();
		}
	}
}
