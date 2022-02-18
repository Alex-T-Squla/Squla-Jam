namespace Squla.Core.IOC
{
	internal abstract class Provider
	{
		protected ObjectGraph graph;

		protected bool isSingleton;

		protected string[] dependentNamedTypes;

		private System.Object _instance;

		public Provider (ObjectGraph graph, bool isSingleton)
		{
			this.graph = graph;
			this.isSingleton = isSingleton;
		}

		public System.Object Instance ()
		{
			if (_instance == null) {
				_instance = CreateInstance ();
			}

			var ins = _instance;
			if (!isSingleton) {
				// set it to null, so it will be created for next request.
				_instance = null;
			}

			return ins;
		}

		protected abstract System.Object CreateInstance ();
	}
}
