using UnityEngine;
using UnityEngine.UI;
using Squla.Core.IOC;

namespace Squla.Core
{
	public class ResponsiveGridLayoutGroup: MonoBehaviourV2
	{
		[System.Serializable]
		public class LayoutData
		{
			public RectOffset padding;
			public Vector2 cellSize;
			public Vector2 spacing;
			public int constraintCount;
		}

		[Tooltip ("This is tablet size.")]
		public LayoutData size_3x4;

		[Inject]
		private IScreenPrefabChooser screenChooser;

		protected override void OnEnable ()
		{
			base.OnEnable ();

			if (screenChooser.WhichOne != ScreenSize.Size_3x4)
				return;

			var layoutGroup = GetComponent<GridLayoutGroup> ();
			if (layoutGroup == null) {
				logger.Warning ("ResponsiveGridLayoutGroup is present but there is no GridLayoutGroup in this GameObject {0}", gameObject.name);
				return;
			}

			ApplyLayout (layoutGroup, size_3x4);
		}

		private void ApplyLayout (GridLayoutGroup layoutGroup, LayoutData data)
		{
			layoutGroup.padding = data.padding;
			layoutGroup.cellSize = data.cellSize;
			layoutGroup.spacing = data.spacing;
			layoutGroup.constraintCount = data.constraintCount;
		}
	}
}
