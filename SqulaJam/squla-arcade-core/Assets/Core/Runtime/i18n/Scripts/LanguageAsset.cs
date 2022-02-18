using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Squla.Core.i18n
{
	public class LanguageAsset : ScriptableObject, IParsedFromExternalFile
	{
		public Pair[] arrayKeys;
		private Dictionary<string, string> _keys;


		private string keyString = "msgid ";
		private string valueString = "msgstr ";

		bool IParsedFromExternalFile.ParseFromExternalFile (string poFilePath)
		{
			bool isMsgId = false;
			bool isMsgStr = false;
			Pair currentKeys = null;
			List<Pair> keys = new List<Pair> ();
			using (StreamReader sr = new StreamReader (poFilePath)) {
				while (!sr.EndOfStream) {
					string line = sr.ReadLine ().Replace ("\\n", "");
					if (!line.StartsWith ("#")) {
						if (String.IsNullOrEmpty (line.Trim ())) {
							if (currentKeys != null) {
								if (String.IsNullOrEmpty (currentKeys.key))
									currentKeys.key = "StartKey";
								keys.Add (currentKeys);
								currentKeys = null;
								isMsgId = false;
								isMsgStr = false;
							}
						}
						if (line.StartsWith (keyString)) {
							isMsgId = true;
							isMsgStr = false;
							if (currentKeys == null || String.IsNullOrEmpty (currentKeys.key)) {
								currentKeys = new Pair ();
								currentKeys.key = TrimStart (line, keyString).Trim ('"');
							} else {
								currentKeys.key += ("\n" + TrimStart (line, keyString).Trim ('"'));
							}
						} else if (line.StartsWith (valueString)) {
							isMsgId = false;
							isMsgStr = true;
							if (String.IsNullOrEmpty (currentKeys.value)) {
								currentKeys.value = TrimStart (line, valueString).Trim ('"');
							} else {
								currentKeys.value += ("\n" + TrimStart (line, valueString).Trim ('"'));
							}
						} else {
							if (isMsgId) {
								currentKeys.key += ("\n" + line.Trim ('"'));
							}
							if (isMsgStr) {
								currentKeys.value += ("\n" + line.Trim ('"'));
							}
						}
					}
				}
			}

			if (currentKeys != null && !keys.Contains (currentKeys)) {
				keys.Add (currentKeys);
			}

			arrayKeys = keys.ToArray ();
			_keys = keys.ToDictionary (x => x.key, x => x.value);
			return true;
		}

		void OnEnable ()
		{
			if (arrayKeys != null) {
				_keys = arrayKeys.ToDictionary (x => x.key, x => x.value);
			} else {
				_keys = new Dictionary<string, string> ();
			}
		}

		bool IParsedFromExternalFile.UpdateFromExternalFile (string poFilePath)
		{
			return ((IParsedFromExternalFile)this).ParseFromExternalFile (poFilePath);
		}

		public string GetTranslation (string key)
		{
			if (!_keys.ContainsKey (key)) {
				return key;
			}

			string result = _keys [key];
			if (String.IsNullOrEmpty (result)) {
				return key;
			}

			return result;
		}

		public static string TrimStart (string target, string trimString)
		{
			string result = target;
			while (result.StartsWith (trimString)) {
				result = result.Substring (trimString.Length);
			}

			return result;
		}

		public static string TrimEnd (string target, string trimString)
		{
			string result = target;
			while (result.EndsWith (trimString)) {
				result = result.Substring (0, result.Length - trimString.Length);
			}

			return result;
		}
	}
}

