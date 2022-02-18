using System;
using UnityEngine;

namespace Squla.Core.Network
{
	public interface IImageDownloader
	{
		Sprite GetImage (string url);

		void GetImage(string url, GameObject go, Action<string, Sprite> onSprite);

		void Download (Batch batch);

		void ReleaseImage (string url);

		void ReleaseImageImmediate (string url);

		void Flush ();
	}
}
