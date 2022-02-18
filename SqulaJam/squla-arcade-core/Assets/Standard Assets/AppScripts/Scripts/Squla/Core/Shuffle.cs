using UnityEngine;
using System.Collections.Generic;

namespace Squla.Core
{
	public static class Shuffle
	{
		public static void KnuthShuffle<T> (IList<T> list)
		{
			int n = list.Count;
			while (n > 1) {
				n--;
				int k = Random.Range (0, n + 1);
				T value = list [k];
				list [k] = list [n];
				list [n] = value;
			}
		}
	}
}

