using UnityEngine;

namespace Squla.Core.Network
{
	internal class IDataResponse_WWW : IDataResponse
	{
		public int ResponseCode { get; private set; }

		private string _responseText;
		public string ResponseText {
			get {
				if (_www == null)
					return _responseText;

				_responseText = _www.text;
				_www.Dispose ();
				_www = null;
				return _responseText;
			}
		}

		private Sprite _sprite;
		public Sprite Sprite {
			get {
				if (_www == null)
					return _sprite;

				var tex = new Texture2D (1, 1, TextureFormat.RGBA32, false);
				tex.wrapMode = TextureWrapMode.Clamp;
				_www.LoadImageIntoTexture (tex);
				var sprite = Sprite.Create (tex, new Rect (0, 0, tex.width, tex.height), new Vector2 (0.5f, 0.5f));

				_www.Dispose ();
				return sprite;
			}
		}

		private AudioClip _audioClip;
		private byte[] _audioBytes;

		public AudioClip AudioClip {
			get {
				if (_www == null)
					return _audioClip;
				ProcessAudio ();
				return _audioClip;
			}
		}

		public byte[] AudioBytes {
			get {
				if (_www == null)
					return _audioBytes;
				ProcessAudio ();
				return _audioBytes;
			}
		}

		private WWW _www;

		internal IDataResponse_WWW (WWW www, int code)
		{
			_www = www;
			ResponseCode = code;
		}

		private void ProcessAudio ()
		{
			_audioClip = _www.GetAudioClip (false, false);
			_audioBytes = _www.bytes;

			_www.Dispose ();
			_www = null;
		}
	}
}
