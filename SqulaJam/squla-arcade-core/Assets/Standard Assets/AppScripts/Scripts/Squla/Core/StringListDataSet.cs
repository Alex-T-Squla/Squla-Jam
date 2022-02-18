using UnityEngine;

namespace Squla.Core
{
	[CreateAssetMenu(fileName = "StringListDataSet", menuName = "TestData/StringListDataSet", order = 109)]
	public class StringListDataSet: ScriptableObject
	{
		public string[] data;
	}
}
