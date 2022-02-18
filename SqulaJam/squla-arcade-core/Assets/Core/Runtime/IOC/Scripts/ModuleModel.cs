using UnityEngine;
using System;

namespace Squla.Core.IOC
{
	[Serializable]
	public class ModuleModel
	{
		public string name;
		public bool enabled;
		public GameObject prefab;
	}
}