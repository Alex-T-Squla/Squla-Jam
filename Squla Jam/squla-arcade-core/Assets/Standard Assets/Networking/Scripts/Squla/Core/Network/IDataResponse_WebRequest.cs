using UnityEngine;
using UnityEngine.Networking;

namespace Squla.Core.Network
{
    internal class IDataResponse_WebRequest: IDataResponse
    {
        public int ResponseCode { get; private set; }

        private string _responseText;
        public string ResponseText {
            get {
                if (_www == null)
                    return _responseText;

                _responseText = _www.downloadHandler.text;
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
                tex.LoadImage (_www.downloadHandler.data);
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

        private UnityWebRequest _www;

        internal IDataResponse_WebRequest (UnityWebRequest www)
        {
            _www = www;
            ResponseCode = (int)www.responseCode;
        }

        internal IDataResponse_WebRequest (UnityWebRequest www, int httpStatus)
        {
	        _www = www;
	        ResponseCode = httpStatus;
        }

        private void ProcessAudio ()
        {
	        if (ResponseCode == 200) {
	            var handler = (DownloadHandlerAudioClip) _www.downloadHandler;
	            _audioClip = handler.audioClip;
	            _audioBytes = handler.data;
	        }

            _www.Dispose ();
            _www = null;
        }
    }
}
