using UnityEngine;

namespace Squla.Core.IOC
{
	[CreateAssetMenu (fileName = "ModuleData", menuName = "Squla/Data/ModuleData", order = 104)]
	public class ModuleModelSet : ScriptableObject
	{
		public ModuleModel[] dataSet;
	}
}