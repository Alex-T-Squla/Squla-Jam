using UnityEngine;
using System.Collections;

namespace Squla.Core
{
	public class ColorConverter
	{
		public static Color HexToColor (string hex)
		{
			byte r = byte.Parse (hex.Substring (2, 2), System.Globalization.NumberStyles.HexNumber);
			byte g = byte.Parse (hex.Substring (4, 2), System.Globalization.NumberStyles.HexNumber);
			byte b = byte.Parse (hex.Substring (6, 2), System.Globalization.NumberStyles.HexNumber);
			byte a = byte.Parse (hex.Substring (8, 2), System.Globalization.NumberStyles.HexNumber);
			return new Color32 (r, g, b, a);
		}
	}
}
