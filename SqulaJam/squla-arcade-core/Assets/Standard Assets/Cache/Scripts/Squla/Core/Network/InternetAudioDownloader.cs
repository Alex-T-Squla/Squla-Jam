using UnityEngine;
using System;
using Squla.Core.IOC;

namespace Squla.Core.Network
{
	[Singleton]
	public class InternetAudioDownloader : IFallbackAudioDownloader
	{
		private readonly AudioRequestManager manager;

		[Inject]
		public InternetAudioDownloader (AudioRequestManager manager)
		{
			this.manager = manager;
		}

		public void GetAudioClip (string url, Action<string, AudioClip, byte[]> OnFinished)
		{
			manager.GET (url, OnFinished);
		}
	}
}
