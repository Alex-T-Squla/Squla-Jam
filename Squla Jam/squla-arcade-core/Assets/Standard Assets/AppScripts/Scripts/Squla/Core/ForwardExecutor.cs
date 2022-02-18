using System;

namespace Squla.Core
{
	public class ForwardExecutor
	{
		private bool conditionMet;
		private Action callback;

		public ForwardExecutor ()
		{
		}

		public void Execute (Action callback)
		{
			if (conditionMet) {
				callback ();
			} else {
				this.callback = callback;
			}
		}

		public void Reset ()
		{
			conditionMet = false;
		}

		public void ConditionMet ()
		{
			conditionMet = true;
			if (callback != null) {
				callback ();
				callback = null;
			}
		}
	}
}

