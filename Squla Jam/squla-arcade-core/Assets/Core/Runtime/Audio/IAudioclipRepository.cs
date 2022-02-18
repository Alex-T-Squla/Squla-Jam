using UnityEngine;

namespace Squla.Core.Audio
{
	public interface IAudioclipRepository
	{
		void Register (AudioClipDataSource dataSource);

		void Register (string clipName, AudioClip clip);

		void Register(AudioSpriteDataSource spriteSource);

		void UnRegister (AudioClipDataSource dataSource);

		void UnRegister (string clipName);

		void UnRegister(AudioSpriteDataSource spriteSource);

		AudioSource GetAudioSource (bool getNullIfNoneAvailable = false);
	}
}
	