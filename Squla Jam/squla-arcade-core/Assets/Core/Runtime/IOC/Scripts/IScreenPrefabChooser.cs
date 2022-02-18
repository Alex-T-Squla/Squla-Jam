using UnityEngine;
using System.Collections;

namespace Squla.Core.IOC
{
	[System.Serializable]
	public enum ScreenSize
	{
		Size_9x16,
		Size_3x4}

	;

	public interface IScreenPrefabChooser
	{
		ScreenSize WhichOne { get; }
	}
}
