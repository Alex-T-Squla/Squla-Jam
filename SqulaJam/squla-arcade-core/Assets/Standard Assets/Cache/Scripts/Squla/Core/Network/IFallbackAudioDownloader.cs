using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Squla.Core.Network
{
	public interface IFallbackAudioDownloader
	{
		void GetAudioClip (string url, Action<string, AudioClip, byte[]> OnFinished);
	}
}
