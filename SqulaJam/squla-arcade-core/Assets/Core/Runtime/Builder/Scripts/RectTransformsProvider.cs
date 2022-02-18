using UnityEngine;
using Squla.Core.Logging;

namespace Squla.Core.IOC.Builder
{
	public class RectTransformsProvider : MonoBehaviour
	{
		private readonly SmartLogger logger = SmartLogger.GetLogger<RectTransformsProvider> ();

		public RectTransformItem[] slots;

		private void Awake ()
		{
			if (slots == null || slots.Length == 0)
				return;

			ObjectGraph graph = ObjectGraph.main;
			if (graph == null) {
				logger.Error ("ObjectGraph is not initialized.");
			} else {
				var path = gameObject.name;
				logger.Debug (string.Format ("Registering RectTransformsProvider at path '{0}'", path));

				for (int i = 0; i < slots.Length; i++) {
					var slot = slots [i];
					if (slot.IsValid) {
						graph.RegisterRectTransformProvider (slot);
					} else {
						logger.Error (string.Format ("RectTransformsProvider component at path '{0}' has empty item", path));
					}
				}
			}
		}

		void OnDestroy ()
		{
			ObjectGraph graph = ObjectGraph.main;
			if (slots == null || slots.Length == 0 || graph == null)
				return;

			logger.Debug (string.Format ("UnRegistering RectTransformsProvider at path '{0}'", gameObject.name));

			for (int i = 0; i < slots.Length; i++) {
				var slot = slots [i];
				if (slot.IsValid) {
					graph.UnRegisterRectTransformProvider (slot);
				}
			}
		}
	}
}
