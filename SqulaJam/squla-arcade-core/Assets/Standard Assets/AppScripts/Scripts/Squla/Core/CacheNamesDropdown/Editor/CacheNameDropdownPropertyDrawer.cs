using System;
using UnityEditor;
using UnityEngine;

namespace Squla.Core
{
	[CustomPropertyDrawer(typeof(CacheNameDropdown))]
	public class CacheNameDropdownPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			CacheNameDropdown cacheNameDropdown = (CacheNameDropdown) attribute;

			EditorGUI.BeginProperty(position, label, property);

			int selected = Array.IndexOf(cacheNameDropdown.list, property.stringValue);

			if (selected < 0 && cacheNameDropdown.list.Length > 0)
				selected = 0;
			int s = EditorGUI.Popup(position, label.text, selected, cacheNameDropdown.list);

			if (selected != s) {
				property.stringValue = cacheNameDropdown.list[s];
			}

			EditorGUI.EndProperty();
		}
	}
}