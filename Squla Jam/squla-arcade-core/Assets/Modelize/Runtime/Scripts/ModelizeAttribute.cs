using System;

namespace Squla.Core.Modelize
{
	public class ModelizeAttribute : Attribute
	{
		public string ModelTypeName { get; private set; }

		public string KeyField { get; private set; }

		public int PoolSize { get; private set; }

		public ModelizeAttribute (string modelTypeName, string keyField, int poolSize = 20)
		{
			ModelTypeName = modelTypeName;
			KeyField = keyField;
			PoolSize = poolSize;
		}
	}
}

