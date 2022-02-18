using Squla.Core.IOC;
using UnityEngine;

public class CanvasSafeArea : MonoBehaviourV2
{
	[Inject("originalScreenSize")] private Rect originalScreenSize;
	[Inject("screenScale")] private float screenScale;
	[SerializeField] private Canvas canvas;
	[SerializeField] private RectTransform SafeAreaRect;

	private Rect lastSafeArea = Rect.zero;

	private void Update()
	{
		if (lastSafeArea != Screen.safeArea) {
			lastSafeArea = Screen.safeArea;
			ApplySafeArea();
		}
	}

	protected void Start()
	{
		lastSafeArea = Screen.safeArea;
		ApplySafeArea();
	}

	void ApplySafeArea()
	{
		var sa = new Rect(Mathf.Round(Screen.safeArea.x), Mathf.Round(Screen.safeArea.y),
			Mathf.Round(Screen.safeArea.width), Mathf.Round(Screen.safeArea.height));
		if (sa == originalScreenSize) {
			return;
		}

		Rect safeArea = Screen.safeArea;

		var difference = originalScreenSize.height * screenScale - safeArea.height;

		var bottom = difference - safeArea.y;
		if (safeArea.y != 0) {
			bottom -= 20f;
		}

		//Bottom
		SafeAreaRect.offsetMin = new Vector2(SafeAreaRect.offsetMin.x, bottom);
		//Top
		SafeAreaRect.offsetMax = new Vector2(SafeAreaRect.offsetMax.x, -safeArea.y - 24); // Margin top

		// Vector2 anchorMin = safeArea.position;
		// Vector2 anchorMax = safeArea.position + safeArea.size;
		// var pixelRect = canvas.pixelRect;
		// anchorMin.x /= pixelRect.width;
		// anchorMin.y /= pixelRect.height;
		// anchorMax.x /= pixelRect.width;
		// anchorMax.y /= pixelRect.height;
		//
		// SafeAreaRect.anchorMin = anchorMin;
		// SafeAreaRect.anchorMax = anchorMax;
	}
}