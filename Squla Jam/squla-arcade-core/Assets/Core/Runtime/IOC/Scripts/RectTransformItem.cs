using UnityEngine;
using System;

namespace Squla.Core.IOC
{
	[Serializable]
	public class RectTransformItem
	{
		public string name;
		public RectTransform target;

		public bool IsValid {
			get {
				return target != null;
			}
		}
	}
}