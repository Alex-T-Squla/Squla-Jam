using System;
using System.IO;
using Squla.Core.IOC;
using Squla.Core.ZeroQ;

namespace Squla.Core.Network
{
	public class L2TextDownloader: ITextDownloader
	{
		private readonly ICoroutineManager manager;
		private readonly INetworkManager networkManager;
		private readonly LRUFileSystem<string> cache;

		private readonly Bus bus;
		
		public L2TextDownloader (int cacheSize, string path, ICoroutineManager manager, [Inject("Asset")]INetworkManager networkManager, Bus bus)
		{
			this.manager = manager;
			this.networkManager = networkManager;
			cache = new LRUFileSystem<string> (cacheSize, path);
			this.bus = bus;
		}

		public void GetText(string url, Action<string> OnFinished)
		{
			Action<string, string> onResponse = (u, s1) => {
				if (string.IsNullOrEmpty(s1)) {
					Download(url, OnFinished);
				} else {
					OnFinished(s1);
				}
			};

			if (cache.HasItem (url)) {
				manager.CreateCoroutine(cache.LoadTextFromFile(url, onResponse));
				return;
			}

			Download(url, OnFinished);
		}

		private void Download(string url, Action<string> onFinished)
		{
			var req = new DataRequest {
				url = url,
				source = onFinished,
				onComplete = (request, response) => {
					var text = response.ResponseText;
					if (!string.IsNullOrEmpty(text)) {
						try {
							cache.Insert(url, text);
							cache.SaveTextToFile(text, url);
						} catch (IOException) {
							bus.Publish("cmd://the-app/storage-warning");
						}
					}
					onFinished(text);
				}
			};

			networkManager.Execute (req);
		}

		public void Flush()
		{
		}
	}
}