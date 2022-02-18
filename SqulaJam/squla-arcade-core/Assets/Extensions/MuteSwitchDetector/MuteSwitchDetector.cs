using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using Squla.Core.IOC;
using UnityEngine.Audio;
using UnityEngine.Events;

public class MuteSwitchDetector : MonoBehaviourV2
{
	#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
	private static extern void StartListeningForMuteChanges();

	[DllImport ("__Internal")]
	private static extern void StopListeningForMuteChanges();

	[DllImport ("__Internal")]
	private static extern bool IsMuted();
	#endif

	public bool isMuted {
		get {
			#if UNITY_IPHONE && !UNITY_EDITOR
			return IsMuted();
			#endif
			return false;
		}
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		CheckMute();
	}

	protected override void OnEnable()
	{
		CheckMute();
	}

	protected override void AfterAwake()
	{
		#if UNITY_IPHONE && !UNITY_EDITOR
		StartListeningForMuteChanges();
		#endif
	}
		
	void OnDestroy() {
		#if UNITY_IPHONE && !UNITY_EDITOR
		StopListeningForMuteChanges();
		#endif
	}

	void OnMuteStateChanged(string message) {
		bool newState = bool.Parse(message);
		bus.Publish(AudioSignals.cmd_Audio_Set_Music_Mute, newState);
	}

	private void CheckMute()
	{
		bus.Publish(AudioSignals.cmd_Audio_Set_Music_Mute, isMuted);
	}
}
