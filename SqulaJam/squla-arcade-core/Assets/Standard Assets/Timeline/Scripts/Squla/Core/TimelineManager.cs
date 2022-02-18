using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Squla.Core.IOC;


namespace Squla.Core
{
	[SingletonModule]
	public class TimelineManager : MonoBehaviourV2, ITimelineManager
	{
		int currentFrame;
		private bool running;

		private List<Timeline> timelines = new List<Timeline> ();

		protected override void AfterAwake ()
		{
			graph.RegisterModule (this);
		}

		[Provides]
		[Singleton]
		public ITimelineManager provideTimelineManager ()
		{
			return this;
		}

		int index = 0;

		void FixedUpdate ()
		{
			if (!running)
				return;

			if (index % 2 == 0) {
				MakeTick ();
				index = 0;
			}
			index++;
		}

		void MakeTick ()
		{
			List<Timeline> timeLinesForRemoval = new List<Timeline> ();

			for (int i = 0; i < timelines.Count; i++) {
				if (timelines [i].frameSubscriptions.ContainsKey (currentFrame)) {
					List<System.Action> actions = timelines [i].frameSubscriptions [currentFrame];

					timelines [i].frameSubscriptions.Remove (currentFrame);
					if (timelines [i].frameSubscriptions.Count == 0) {
						timeLinesForRemoval.Add (timelines [i]);
					}

					for (int j = 0; j < actions.Count; j++) {
						try {
							actions[j]?.Invoke();
						} catch (Exception ex) {
							logger.Error ("callback action thrown exception {0}", ex);
						}
					}
				}

				if (timelines.Count == 0) {
					Stop ();
				}
			}

			for (int i = 0; i < timeLinesForRemoval.Count; i++) {
				timelines.Remove (timeLinesForRemoval [i]);
			}
			
			currentFrame++;
		}

		private int FindLatestFrame ()
		{
			int highestFrame = 0;
			for (int i = 0; i < timelines.Count; i++) {
				//TODO: check for Count 0 in Dictionary before doing MAx
				if (timelines [i].frameSubscriptions.Keys.Count > 0 && highestFrame < timelines [i].frameSubscriptions.Keys.Max ()) {
					highestFrame = timelines [i].frameSubscriptions.Keys.Max ();
				}
			}

			return highestFrame;
		}

		private void Start ()
		{
			running = true;
		}

		private void Stop ()
		{
			running = false;
		}

		public void Append (System.Action action, int frame)
		{

			int largest = 0;

			for (int i = 0; i < timelines.Count; i++) {
				var val = timelines [i].frameSubscriptions.Keys.Count > 0 ? timelines [i].frameSubscriptions.Keys.Max () : 0;

				if (val > largest) {
					largest = val;
				}
			}

			if (largest == 0) {
				largest = currentFrame;
			}
			Timeline timeline = new Timeline (action, frame + largest);
			timelines.Add (timeline);
			Start ();
		}

		public void Delay (int frame)
		{
			Timeline timeline = new Timeline (() => {
			}, frame + currentFrame);
			timelines.Add (timeline);
			Start ();
		}

		public void Add (System.Action action, int frame)
		{
			Timeline timeline = new Timeline (action, frame + currentFrame);
			timelines.Add (timeline);
			Start ();
		}

		public void Add (System.Action[] actions, int staggerDelay, int frame)
		{
			Timeline timeline = new Timeline (actions, staggerDelay, frame + currentFrame);
			timelines.Add (timeline);
			Start ();
		}

		public void Add (System.Action action, string increase = "0")
		{

			int addedTime = 0;
			Int32.TryParse (increase, out addedTime);
			Timeline timeline = new Timeline (action, FindLatestFrame () + addedTime);
			timelines.Add (timeline);
			Start ();
		}

		public void Clear()
		{
			timelines.Clear();
		}
	}
}