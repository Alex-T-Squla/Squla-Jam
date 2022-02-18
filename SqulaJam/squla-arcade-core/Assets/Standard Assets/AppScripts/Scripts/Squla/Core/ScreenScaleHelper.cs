using UnityEngine;
using UnityEngine.UI;

namespace Squla.Core
{
	public class ScreenScaleHelper
	{
		private RectTransform canvas;
		private Rect screenSize;

		public ScreenScaleHelper(Canvas canvas, Rect screenSize)
		{
			this.canvas = (RectTransform) canvas.transform;
			this.screenSize = screenSize;
		}

		// We take the screen pixels and convert it to the percentage of the canvas height to get the pixels in canvas units
		public float ScreenSizeToCanvas(int pixels)
		{
			return pixels / screenSize.height * canvas.rect.height;
		}
	}
}
