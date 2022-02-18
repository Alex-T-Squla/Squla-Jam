using Squla.Core.ZeroQ;

namespace Squla.Core.IOC
{
	public class Laizy<T>  where T : class
	{
		private string name;
		private T target;

		internal Laizy (string name)
		{
			this.name = name;
		}

		public T Get ()
		{
			Prepare ();

			return target;
		}

		public T Create ()
		{
			var graph = ObjectGraph.main;
			return graph.Get<T> (name);
		}

		public void Destroy ()
		{
			if (target == null)
				return;

			var graph = ObjectGraph.main;
			var bus = graph.Get<Bus> ();
			bus.UnRegister (target);
		}

		private void Prepare ()
		{
			if (target != null)
				return;

			var graph = ObjectGraph.main;
			target = graph.Get<T> (name);
		}
	}
}
