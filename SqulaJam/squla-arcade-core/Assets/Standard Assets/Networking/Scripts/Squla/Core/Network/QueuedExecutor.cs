using System;
using System.Collections;
using System.Collections.Generic;

namespace Squla.Core.Network
{
	public class QueuedExecutor
	{
		private readonly ICoroutineManager manager;

		private readonly Queue<Func<IEnumerator>> queue = new Queue<Func<IEnumerator>> ();

		private const int L2_LOAD_SPEED_FACTOR = 10;

		public QueuedExecutor (ICoroutineManager manager)
		{
			this.manager = manager;
		}

		public void AddToQueue (Func<IEnumerator> action)
		{
			queue.Enqueue (action);
			if (queue.Count > 1) {
				return;
			}

			manager.CreateCoroutine (Worker ());
		}

		public void Flush ()
		{
			queue.Clear ();
		}

		private IEnumerator Worker ()
		{
			var count = 0;
			while (queue.Count != 0) {
				var action = queue.Peek ();
				var task = action ();
				if (task == null)
					continue;

				yield return task;

				count++;

				if (count % L2_LOAD_SPEED_FACTOR == 0) {
					// skip for a frame to give time to update the UI.
					yield return null;
				}

				// we remove it in the next frame this is safe
				queue.Dequeue ();
			}
		}
	}
}