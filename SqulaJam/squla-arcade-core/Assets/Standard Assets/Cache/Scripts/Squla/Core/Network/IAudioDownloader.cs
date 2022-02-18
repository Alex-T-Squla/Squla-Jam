using UnityEngine;
using System;

namespace Squla.Core.Network
{
	public interface IAudioDownloader
	{
		AudioClip GetAudioClip (string url);

		void GetAudioClip(string url, GameObject go, Action<string, AudioClip> onAudioClip);

		void Download (Batch batch);

		void ReleaseAudio (string url);

		void Flush ();
	}
}
