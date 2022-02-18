using UnityEngine;

namespace Squla.Core.Network
{
	internal class IDataResponse_Null : IDataResponse
	{
		public int ResponseCode { get; private set; }

		public string ResponseText {
			get { return string.Empty; }
		}

		public Sprite Sprite {
			get { return null; }
		}

		public AudioClip AudioClip {
			get { return null; }
		}

		public byte[] AudioBytes {
			get { return null; }
		}

		internal IDataResponse_Null (int code)
		{
			ResponseCode = code;
		}
	}
}
