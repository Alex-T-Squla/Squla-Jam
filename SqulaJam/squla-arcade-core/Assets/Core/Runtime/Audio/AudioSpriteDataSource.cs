using UnityEngine;
using Squla.Core.IOC;
using System;

namespace Squla.Core.Audio
{
	public class AudioSpriteDataSource : MonoBehaviourV2
	{
		public string _name;

		public AudioClip audioSprite;

		public SpriteConfig spriteConfig;

		[Inject]
		private IAudioclipRepository repository;

		protected override void AfterAwake ()
		{
			logger.Debug ("Working with: {0}", repository);
			
			// this not null check is for editor to not to report.
			if (repository != null) {
				repository.Register (this);
			}
		}

		public bool Initialize(string dsName, AudioClip audioClip, string configJson)
		{
			_name = dsName;
			if (!string.IsNullOrEmpty(configJson) && audioClip != null) {
				audioSprite = audioClip;
				
				spriteConfig = JsonUtility.FromJson<SpriteConfig>(configJson);
				SetSpriteEventAudioClip(spriteConfig, audioClip);
			}
			var isInitialized = spriteConfig != null && spriteConfig.sprite != null && audioSprite != null;
			return isInitialized;
		}

		private void SetSpriteEventAudioClip(SpriteConfig config, AudioClip audioClip)
		{
			if (config == null || config.sprite == null) return;
			for (var i = 0; i < config.sprite.Length; i++) {
				var spriteEvent = config.sprite[i];
				spriteEvent.audioClip = MakeSubclip(audioClip, spriteEvent.time);
			}
		}
		
		private AudioClip MakeSubclip(AudioClip clip, SpriteEvent.SpriteEventTime time)
		{
			/* Create a new audio clip */
			int frequency = clip.frequency;
			float timeLength = time.end * 0.001f;
			int samplesLength = (int)(frequency * timeLength);
			AudioClip newClip = AudioClip.Create(clip.name + "-sub", samplesLength, 1, frequency, false);
			/* Create a temporary buffer for the samples */
			float[] data = new float[samplesLength];
			/* Get the data from the original clip */
			clip.GetData(data, (int)(frequency * (time.start * 0.001f)));
			/* Transfer the data to the new clip */
			newClip.SetData(data, 0);
			/* Return the sub clip */
			return newClip;
		}

		void OnDestroy ()
		{
			// this not null check is for editor to not to report.
			if (repository != null) {
				repository.UnRegister (this);
			}
		}
		
		[Serializable]
		public class SpriteConfig
		{
			public string[] urls;
			public SpriteEvent[] sprite;
		}
	}
	
	
}
