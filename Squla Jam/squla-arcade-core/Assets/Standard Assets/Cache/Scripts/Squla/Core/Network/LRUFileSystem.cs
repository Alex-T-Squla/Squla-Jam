using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;

namespace Squla.Core.Network
{
	public class LRUFileSystem<V>
	{
		public delegate void ItemDeleted (string key, LRUFileSystem<V> cache);

		public event ItemDeleted OnItemDeleted;

		private readonly int _maxCapacity = 0;
		private readonly List<string> _LRUCache;
		private readonly string _path;

		public LRUFileSystem (int argMaxCapacity, string path, string searchPattern = "")
		{
			if (!Directory.Exists (path)) {
				Directory.CreateDirectory (path);
			}

			_maxCapacity = argMaxCapacity;
			_path = path;
			DirectoryInfo d = new DirectoryInfo (_path);
			if (!String.IsNullOrEmpty (searchPattern)) {
				_LRUCache = d.GetFiles (searchPattern).OrderBy (x => x.LastAccessTime).Select (x => x.Name).ToList ();
			} else {
				_LRUCache = d.GetFiles ().OrderBy (x => x.LastAccessTime).Select (x => x.Name).ToList ();
			}
		}

		public void Insert (string key, V value)
		{
			string fileName = UrlToFileName (key);
			if (_LRUCache.Contains (fileName)) {
				MakeMostRecentlyUsed (fileName);
				return;
			}

			if (_LRUCache.Count >= _maxCapacity)
				RemoveLeastRecentlyUsed ();

			_LRUCache.Add (fileName);

			if (typeof(V) == typeof(Sprite)) {
				Sprite v = value as Sprite;
				SaveTextureToFile (v.texture, key);
			}
		}
		
		public void SaveBytesToFile (byte[] bytes, string url)
		{
			var file = File.Open(_path + "/" + UrlToFileName(url), FileMode.Create);
			var binary = new BinaryWriter(file);
			binary.Write(bytes);
			binary.Flush();
			file.Close();
		}

		private void SaveTextureToFile (Texture2D texture, string url)
		{
			SaveBytesToFile(texture.EncodeToPNG(), url);
		}
		
		public void SaveTextToFile (string text, string url)
		{
			SaveBytesToFile(Encoding.UTF8.GetBytes(text), url);
		}

		public IEnumerator LoadSpriteFromFile (string url, Action<string, Sprite> CallerFinished)
		{
			var key = UrlToFileName(url);
			string file = "file://" + _path + "/" + key;
			WWW www = new WWW (file);
			yield return www;

			var tex = new Texture2D (1, 1, TextureFormat.RGBA32, false);
			tex.wrapMode = TextureWrapMode.Clamp;
			tex.name = url;
			www.LoadImageIntoTexture (tex);
			var sprite = Sprite.Create (tex, new Rect (0, 0, tex.width, tex.height), new Vector2 (0.5f, 0.5f));

			if (String.IsNullOrEmpty (www.error)) {
				CallerFinished (url, sprite);
			} else {
				RemoveKey(key);
				CallerFinished (url, null);
			}

			www.Dispose ();
		}

		public bool HasItem (string url)
		{
			return _LRUCache.Contains (UrlToFileName (url));
		}

		public IEnumerator LoadClipFromFile (string url, Action<string, AudioClip, byte[]> CallerFinished)
		{
			var key = UrlToFileName(url);
			string file = "file://" + _path + "/" + key;
			file = file.Replace (" ", "%20");
			WWW www = new WWW (file);
			yield return www;
			AudioClip audioClip = www.GetAudioClip (false, false);
			byte[] bytes = www.bytes;
			if (String.IsNullOrEmpty (www.error)) {
				CallerFinished (url, audioClip, bytes);
			} else {
				RemoveKey(key);
				CallerFinished (url, null, null);
			}
			www.Dispose();
		}

		public IEnumerator LoadTextFromFile(string url, Action<string, string> onFinished)
		{
			var key = UrlToFileName(url);
			var file = "file://" + _path + "/" + key;
			var www = new WWW (file);
			yield return www;

			if (string.IsNullOrEmpty(www.error))
				onFinished(url, www.text);
			else {
				RemoveKey(key);
				onFinished(url, null);
			}
			www.Dispose();
		}

		public int Size ()
		{
			return _LRUCache.Count;
		}

		private void RemoveKey(string key)
		{
			var index = _LRUCache.IndexOf(key);
			if (index != -1) {
				_LRUCache.RemoveAt(index);
				var path = $"{_path}/{key}";
				if (File.Exists(path))
					File.Delete($"{_path}/{key}");
				if (OnItemDeleted != null) {
					OnItemDeleted (key, this);
				}
			}
		}

		private void RemoveLeastRecentlyUsed ()
		{
			RemoveKey(_LRUCache [0]);
		}

		private void MakeMostRecentlyUsed (string foundItem)
		{
			_LRUCache.Remove (foundItem);
			_LRUCache.Add (foundItem);
			File.SetLastAccessTime (_path + "/" + foundItem, System.DateTime.Now);
		}

		public static string UrlToFileName (string url)
		{
			//return url.Substring (url.LastIndexOf ("/") + 1);

			var parts = url.Split ('/');
			int i1 = parts.Length - 3, i2 = parts.Length - 2, i3 = parts.Length - 1;
			var path = parts [i2] + parts [i3];
			return parts.Length >= 3 ? parts [i1] + path : path;
		}
	}
}
