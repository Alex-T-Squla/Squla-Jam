using UnityEngine;
using System;

namespace Squla.Core.Network
{
	public interface IFallbackImageDownloader
	{
		void GetImage (string url, Action<string, Sprite> OnFinished);

		void Flush ();
	}
}