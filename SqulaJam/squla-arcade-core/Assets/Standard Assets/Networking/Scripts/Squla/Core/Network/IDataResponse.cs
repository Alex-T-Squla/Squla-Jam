using UnityEngine;

namespace Squla.Core.Network
{
	public interface IDataResponse
	{
		int ResponseCode { get; }

		string ResponseText { get; }

		Sprite Sprite { get; }

		AudioClip AudioClip { get; }

		byte[] AudioBytes { get; }
	}
}
