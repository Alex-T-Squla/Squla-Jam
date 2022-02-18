using UnityEngine;
using System;
using Squla.Core.Logging;

namespace Squla.Core.Network
{
	public class Batch
	{
		private static SmartLogger logger = SmartLogger.GetLogger<Batch> ();

		private Action OnCompleted;

		public string[] urls { get; private set; }

		private int remaining;

		public bool isCompletedWithoutErrors { get; private set; }


		public Batch (string[] urls, System.Action OnCompleted)
		{
			isCompletedWithoutErrors = true;
			this.OnCompleted = OnCompleted;
			InitUrls (urls);
		}

		public void OnItemFinished (string url, Sprite sprite)
		{
			if (sprite == null)
				OnFailed ();
			else
				OnSuccess ();
		}

		public void OnItemFinished (string url, AudioClip clip, byte[] bytes)
		{
			if (clip == null)
				OnFailed ();
			else
				OnSuccess ();
		}

		public void OnSuccess ()
		{
			remaining--;
			if (remaining < 0) {
				logger.Error ("Why this happens. trace and fix it");
			}
			if (remaining == 0) {
				OnCompleted ();
			}
		}

		public void OnFailed ()
		{
			isCompletedWithoutErrors = false;
			remaining--;
			if (remaining == 0) {
				OnCompleted ();
			}
		}

		private void InitUrls (string[] urls)
		{
			var validURLCounts = urls.Length;

			// NOTE: This Batch class is very frquently used.
			// For Memory optimization reason, no Linq style
			// coding is done.  By this explict for iteration
			// we consume less memory than Linq construcing 
			// lot of iterator objects to solve the problem.
			for (int i = 0; i < urls.Length; i++) {
				if (string.IsNullOrEmpty (urls [i]))
					validURLCounts--;
			}

			if (validURLCounts == 0) {
				OnCompleted ();
			}

			this.urls = new string[validURLCounts];
			for (int i = 0, j = 0; i < urls.Length; i++) {
				var url = urls [i];
				if (!string.IsNullOrEmpty (url)) {
					this.urls [j++] = url;
				}
			}
			remaining = validURLCounts;
		}
	}
}
