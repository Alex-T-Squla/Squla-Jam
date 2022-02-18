using UnityEngine;

namespace Squla.Core.Audio
{
	public struct AudioEvent
	{
		public AudioClip clip;
		public string clipName;
		public float volumeFactor;
		public float pitchFactor;
	}
}