using UnityEngine;
using Squla.Core.IOC;
using Squla.Core;
using System;

namespace Squla.TDD
{
	public class ux_Timeline : MonoBehaviourV2
	{
		[Inject]
		private ITimelineManager timeline;

		private long start;

		protected override void AfterAwake ()
		{
			Debug.Log ("SUBSCRIBE");
			start = ToUnixTime ();
			timeline.Add (CallMe, 300);
			timeline.Add (CallMe2, 300);
			timeline.Add (CallMe3, 400);
		}

		public void AppendLater ()
		{
			Debug.Log ("APPEND LATER");
			timeline.Append (CallMe4, 40);
			timeline.Append (CallMe4, 40);
			timeline.Append (CallMe4, 40);
			timeline.Append (CallMe4, 40);

		}

		private long diff ()
		{
			return ToUnixTime () - start;
		}

		public void CallMe ()
		{
			Debug.Log ("CALL FROM ANIMATION  " + diff ());
		}

		public void CallMe2 ()
		{
			Debug.Log ("CALL 2 FROM ANIMATION  " + diff ());
		}

		public void CallMe3 ()
		{
			Debug.Log ("CALL 3 FROM ANIMATION " + diff ());
		}

		public void CallMe4 ()
		{
			Debug.Log ("CALL 4 FROM ANIMATION " + diff ());
		}


		public static long ToUnixTime ()
		{
			return (DateTime.UtcNow.Ticks - 621355968000000000) / 100000;
		}
	}
}
