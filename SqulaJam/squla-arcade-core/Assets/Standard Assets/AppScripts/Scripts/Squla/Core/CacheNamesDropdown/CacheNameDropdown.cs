using Squla.Core.Network;
using UnityEngine;

namespace Squla.Core
{
	public class CacheNameDropdown : PropertyAttribute
	{
		public string[] list;

		public CacheNameDropdown()
		{
			list = CacheNames.CacheNamesList;
		}
	}
}