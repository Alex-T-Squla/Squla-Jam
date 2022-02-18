using UnityEngine;
using System;

namespace Squla.Core.IOC
{
	[Serializable]
	public class DataProviderItem
	{
		public string name;
		public ScriptableObject target;

		public bool IsValid {
			get {
				return target != null;
			}
		}
	}
}