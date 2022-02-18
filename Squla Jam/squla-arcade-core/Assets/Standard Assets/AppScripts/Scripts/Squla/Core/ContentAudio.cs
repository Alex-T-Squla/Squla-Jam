using Squla.Core.IOC;
using Squla.Core.Network;
using UnityEngine;

namespace Squla.Core
{
	public class ContentAudio: MonoBehaviourV2
	{
		[SerializeField]
		[CacheNameDropdown]
		private string cacheName;

		private IAudioDownloader downloader;

		private string model;

		private GameObject _gs;

		public void ApplyModel(string audioUrl)
		{
			model = audioUrl;
			Download(play:false);
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			Download (false);
		}

		protected override void OnDisable()
		{
			if (downloader != null) {
				downloader.ReleaseAudio (model);
			}
			base.OnDisable();
		}

		private void Download(bool play=false)
		{
			if (string.IsNullOrEmpty(model) && play) {
				bus.Publish (AudioSignals.cmd_Play_Audio_Sfx, "sfx://button/forward");
			}

			if (string.IsNullOrEmpty(model) && !play)
				return;

			if (downloader == null) {
				_gs = gameObject;
				downloader = graph.Get<IAudioDownloader>(cacheName);
			}

			downloader.GetAudioClip(model, _gs, (url, clip) => {
				if (play && model == url) {
					bus.Publish (AudioSignals.cmd_Play_Audio_Clip, clip);
				}
			});
		}

		public void WhenClicked()
		{
			Download(play:true);
		}
	}
}