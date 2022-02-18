using System;

namespace Squla.Core.Network
{
	public interface ITextDownloader
	{
		void GetText(string url, Action<string> onSprite);

		void Flush ();
	}
}