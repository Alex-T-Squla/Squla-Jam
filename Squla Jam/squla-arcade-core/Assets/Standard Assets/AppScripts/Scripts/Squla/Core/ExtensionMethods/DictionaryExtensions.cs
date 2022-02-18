using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Squla.Core.ExtensionMethods
{
	public static class DictionaryExtensions
	{
		// http://stackoverflow.com/a/2601501 see for implementation details and discussion
		public static TValue GetValueOrDefault<TKey, TValue>
		(this IDictionary<TKey, TValue> dictionary, 
		 TKey key,
		 TValue defaultValue)
		{
			TValue value;
			return dictionary.TryGetValue (key, out value) ? value : defaultValue;
		}

		// Clone a dictionary and values: http://stackoverflow.com/a/139841
		// Note if the values do not need to be cloned, use dictionary consructor overload which takes
		// existing IDictionary as argument.
		public static Dictionary<TKey, TValue> CloneDictionaryCloningValues<TKey, TValue>
		(this Dictionary<TKey, TValue> original) where TValue : ICloneable
		{
			Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue> (original.Count, original.Comparer);

			foreach (KeyValuePair<TKey, TValue> entry in original) {
				ret.Add (entry.Key, (TValue)entry.Value.Clone ());
			}

			return ret;
		}
	}
}
