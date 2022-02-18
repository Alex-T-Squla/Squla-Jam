using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace Squla.Core
{
	public class Timeline
	{

		public Dictionary<int, List<System.Action>> frameSubscriptions = new Dictionary<int, List<System.Action>> ();

		public Timeline (System.Action callback, int frame)
		{
			Call (frame, callback);
		}

		public Timeline (System.Action[] callback, int staggerDelay, int frame)
		{
			StaggerCall (frame, staggerDelay, callback);
		}

		public void StaggerCall (int frame, int staggerDelay, System.Action[] callback)
		{
			for (int i = 0; i < callback.Length; i++) {
				Call (frame + staggerDelay * i, callback [i]);
			}
		}

		public void Call (int frame, System.Action callback)
		{
			if (callback == null) {
				Debug.LogWarning("Tried to register a null callback");
				return;
			}
			if (!frameSubscriptions.ContainsKey (frame)) {
				frameSubscriptions.Add (frame, new List<System.Action> ());
			}

			var list = frameSubscriptions [frame];
			list.Add (callback);
		}
	}
}