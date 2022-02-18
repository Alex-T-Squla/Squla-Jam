using System;
using System.Collections.Generic;
using UnityEngine;
using Squla.Core.Logging;
using Squla.Core.IOC;

namespace Squla.Core.IOC.Builder
{
	public class DataSetsProvider : MonoBehaviour
	{
		protected static SmartLogger logger = SmartLogger.GetLogger<DataSetsProvider> ();

		public DataProviderItem[] dataSets;

		void Awake ()
		{
			if (dataSets == null || dataSets.Length == 0)
				return;

			ObjectGraph graph = ObjectGraph.main;
			if (graph == null) {
				logger.Error ("ObjectGraph is not initialized.");
			} else {
				var path = gameObject.name;
				logger.Debug (string.Format ("Registering data provider at path '{0}'", path));

				for (int i = 0; i < dataSets.Length; i++) {
					var dataSet = dataSets [i];
					if (dataSet.IsValid) {
						graph.RegisterDataProvider (dataSet);
					} else {
						logger.Error (string.Format ("DataSetsProvider component at path '{0}' has empty item", path));
					}
				}
			}
		}

		void OnDestroy ()
		{
			ObjectGraph graph = ObjectGraph.main;
			if (dataSets == null || dataSets.Length == 0 || graph == null)
				return;

			logger.Debug (string.Format ("UnRegistering data provider at path '{0}'", gameObject.name));

			for (int i = 0; i < dataSets.Length; i++) {
				var dataSet = dataSets [i];
				if (dataSet.IsValid) {
					graph.UnRegisterDataProvider (dataSet);
				}
			}
		}
	}
}