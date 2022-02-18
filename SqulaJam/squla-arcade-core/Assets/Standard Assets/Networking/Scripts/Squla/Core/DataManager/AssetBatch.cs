using System.Collections.Generic;
using Squla.Core.IOC;
using Squla.Core.Network;
using WebSocketSharp;

namespace Squla.Core.DataManager
{
    internal class AssetBatch
    {
        private Dictionary<string, List<string>> imageUrls;
        private Dictionary<string, List<string>> audioUrls;

        private int batchCount;
        private readonly System.Action onComplete;

        public AssetBatch(System.Action onComplete)
        {
            this.onComplete = onComplete;
        }

        public void AddImageUrl(string cacheName, string url)
        {
            if (url.IsNullOrEmpty())
                return;

            Init(cacheName, isImage:true);
            imageUrls[cacheName].Add(url);
        }

        public void AddAudioUrl(string cacheName, string url)
        {
            if (url.IsNullOrEmpty())
                return;

            Init(cacheName, isImage:false);
            audioUrls[cacheName].Add(url);
        }

        public void Download(ObjectGraph graph)
        {
            batchCount++;
            CreateImageBatches(graph);
            CreateAudioBatches(graph);
            OnBatchComplete();
        }

        private void CreateImageBatches(ObjectGraph graph)
        {
            if (imageUrls == null)
                return;

            foreach (var item in imageUrls) {
                batchCount++;
                var batch = new Batch(item.Value.ToArray(), OnBatchComplete);
                var downloader = graph.Get<IImageDownloader>(item.Key);
                downloader.Download(batch);
            }
        }

        private void CreateAudioBatches(ObjectGraph graph)
        {
            if (audioUrls == null)
                return;

            foreach (var item in audioUrls) {
                batchCount++;
                var batch = new Batch(item.Value.ToArray(), OnBatchComplete);
                var downloader = graph.Get<IAudioDownloader>(item.Key);
                downloader.Download(batch);
            }
        }

        private void OnBatchComplete()
        {
            batchCount--;
            if (batchCount == 0 && onComplete != null) {
                onComplete();
            }
        }

        private void Init(string cacheName, bool isImage = false)
        {
            if (isImage && imageUrls == null)
                imageUrls = new Dictionary<string, List<string>>();

            if (!isImage && audioUrls == null)
                audioUrls = new Dictionary<string, List<string>>();

            var urls = isImage ? imageUrls : audioUrls;

            if (!urls.ContainsKey(cacheName)) {
                urls[cacheName] = new List<string>();
            }
        }
    }
}