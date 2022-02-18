using UnityEngine;
using UnityEngine.UI;
using Squla.Core.IOC;

namespace Squla.Core
{
	public class ResponsiveLayoutElement: MonoBehaviourV2
	{
		[System.Serializable]
		public class LayoutData
		{
			public float minWidth;
			public float minHeight;
			public float preferredWidth;
			public float preferredHeight;
			public float flexibleWidth;
			public float flexibleHeight;
		}

		[Tooltip ("This is tablet size. Flxeible and Preferred sizes can be safely be 0 if it is not necessary. minSize rule over rules.")]
		public LayoutData size_3x4;

		[Inject]
		private IScreenPrefabChooser screenChooser;

		protected override void AfterAwake ()
		{
			if (screenChooser.WhichOne != ScreenSize.Size_3x4)
				return;

			var layoutElement = GetComponent<LayoutElement> ();
			if (layoutElement == null) {
				logger.Warning ("ResponsiveLayoutElement is present but there is no LayoutElement in this GameObject {0}", gameObject.name);
				return;
			}

			ApplyLayout (layoutElement, size_3x4);
		}

		private void ApplyLayout (LayoutElement layoutElement, LayoutData data)
		{
			layoutElement.minWidth = data.minWidth;
			layoutElement.minHeight = data.minHeight;

			// Note: this is safe to assign data.preferredWidth even if it is zero.
			layoutElement.preferredWidth = data.preferredWidth;
			layoutElement.preferredHeight = data.preferredHeight;

			layoutElement.flexibleWidth = data.flexibleWidth;
			layoutElement.flexibleHeight = data.flexibleHeight;
		}
	}
}

