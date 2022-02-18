using UnityEngine;
using UnityEngine.UI;
using Squla.Core.IOC;

namespace Squla.Core
{
	public class ResponsiveVerticalLayoutGroup: MonoBehaviourV2
	{
		[System.Serializable]
		public class LayoutData
		{
			public RectOffset padding;
			public int spacing;
		}

		[Tooltip ("This is tablet size.")]
		public LayoutData size_3x4;

		[Inject]
		private IScreenPrefabChooser screenChooser;

		protected override void AfterAwake ()
		{
			if (screenChooser.WhichOne != ScreenSize.Size_3x4)
				return;

			var layoutGroup = GetComponent<VerticalLayoutGroup> ();
			if (layoutGroup == null) {
				logger.Warning ("ResponsiveVerticalLayoutGroup is present but there is no VerticalLayoutGroup in this GameObject {0}", gameObject.name);
				return;
			}

			ApplyLayout (layoutGroup, size_3x4);
		}

		private void ApplyLayout (VerticalLayoutGroup layoutGroup, LayoutData data)
		{
			layoutGroup.padding = data.padding;
			layoutGroup.spacing = data.spacing;
		}
	}
}
