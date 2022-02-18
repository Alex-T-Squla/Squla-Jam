using UnityEngine;
using System.Collections;
using Squla.Core.IOC;
using Squla.Core;

public class TimelineLowFPSTest : MonoBehaviourV2
{
	[Inject]
	private ITimelineManager timeline;

	float prevTime;
	float currentTime;
	float sumOfDifferences = 0;

	protected override void AfterAwake ()
	{
		QualitySettings.vSyncCount = 0;  // VSync must be disabled
		Application.targetFrameRate = 12;
		prevTime = Time.time;
		for (int i = 0; i < 30; i++) {
			int j = i;
			timeline.Append (() => {
				currentTime = Time.time;
				sumOfDifferences += (currentTime - prevTime);
				Debug.LogErrorFormat ("Iteration: {0}, Current time: {1}, Previous time: {2}, difference: {3}, sumOfDifferences: {4}", j, currentTime, prevTime, currentTime - prevTime, sumOfDifferences);
				prevTime = currentTime;
			}, 1);
		}
	}
}
