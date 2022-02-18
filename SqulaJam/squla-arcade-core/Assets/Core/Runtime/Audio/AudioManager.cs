using UnityEngine;
using System.Collections.Generic;
using Squla.Core.IOC;
using Squla.Core.ZeroQ;
using System;
using System.Collections;
using DG.Tweening;
using UnityEngine.Audio;

namespace Squla.Core.Audio
{
	[SingletonModule]
	public class AudioManager : MonoBehaviourV2, IAudioclipRepository
	{
		public AudioSource musicAudioSource;
		public AudioSource contextAudioSource;
		public AudioSource[] audioSources;
		public AudioSource oneShotAudioSource;
		public AudioMixer musicMixer;
		public AudioMixer soundMixer;
		public bool savedClipWasPlaying;

		private Dictionary<string, float> _lastPlayTimes = new Dictionary<string, float> ();
		private string nextClip;
		private Dictionary<string, AudioClip> _repository = new Dictionary<string, AudioClip> ();
		private Dictionary<string, SpriteEvent> _spriteEventRepository =
			new Dictionary<string, SpriteEvent>();
		private bool musicMuted;
		private AudioClip currentMusicClip;
		private AudioClip savedClip;
		private Tween fadeOutTween;
		private float savedTime;
		
		private const float volumeMultiplier = 80f;
		private static WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
		private static WaitForSeconds waitSecond = new WaitForSeconds(1f);

		[Provides]
		[Singleton]
		public IAudioclipRepository provideRepository ()
		{
			return this;
		}

		protected override void AfterAwake ()
		{
			graph.RegisterModule (this);

			if (PlayerPrefs.HasKey("musicVolume"))
			{
				changeMusicVolume(PlayerPrefs.GetFloat("musicVolume"));
			}
			else
			{
				changeMusicVolume(1f);
			}

			if (PlayerPrefs.HasKey("soundVolume"))
			{
				changeSoundVolume(PlayerPrefs.GetFloat("soundVolume"));
			}
			else
			{
				changeSoundVolume(1f);
			}
		}

		[Subscribe(AudioSignals.cmd_Back_To_Menu_Music)]
		public void BackToMenuMusic()
		{
			if (!musicAudioSource.isPlaying) {
				MusicMenuPlay(AudioSignals.CLIP_MUSIC_MENU);
			}
		}

		//if you play music, you always start from 0
		[Subscribe(AudioSignals.cmd_Play_Music)]
		public void MusicMenuPlay(string clipName)
		{
			if (string.IsNullOrEmpty(clipName) || !_repository.ContainsKey (clipName)) {
				logger.Error ("clip not found '{0}'", clipName);
				return;
			}

			musicAudioSource.clip = currentMusicClip = _repository [clipName];
			musicAudioSource.time = 0;
			musicAudioSource.Play ();
		}

		[Subscribe(AudioSignals.cmd_Play_Temporary_Music)]
		public void PlayTemporaryMusic(string clipName)
		{
			if (string.IsNullOrEmpty (clipName) || !_repository.ContainsKey (clipName)) {
				logger.Error ("clip not found '{0}'", clipName);
				return;
			}
			
			if (musicAudioSource.clip) {
				savedClip = musicAudioSource.clip;
				savedTime = musicAudioSource.time;
				savedClipWasPlaying = musicAudioSource.isPlaying;
			}

			musicAudioSource.clip = currentMusicClip = _repository [clipName];
			musicAudioSource.time = 0;
			musicAudioSource.Play ();
		}

		//if you switch music, you start from wherever you stopped last time
		[Subscribe (AudioSignals.cmd_Switch_Music)]
		public void MusicMenuFadeOut (AudioSwitchModel model)
		{
			string currentClipName = model.currentClipName;
			string nextClipName = model.nextClipName;
			float time = model.time;

			if (musicAudioSource.isPlaying && musicAudioSource.clip.name == _repository [currentClipName].name && String.IsNullOrEmpty (this.nextClip)) {
				FadeOut (nextClipName, time);
				return;
			}
			if (!musicAudioSource.isPlaying) {
				musicAudioSource.clip = _repository [nextClipName];
				SetTime ();
				musicAudioSource.Play ();
			}
		}

		void FadeOut(string nextClipName, float time)
		{
			nextClip = nextClipName;
			float defaultVolume = musicAudioSource.volume;
			GetLastPlayTime(false);
			fadeOutTween = DOTween.Sequence()
				.Append(musicAudioSource.DOFade(0f, time * 0.5f))
				.AppendCallback(()
					=>
				{
					musicAudioSource.clip = currentMusicClip = _repository[nextClipName];
					SetTime();
					musicAudioSource.Play();
					nextClip = null;
				})
				.Append(musicAudioSource.DOFade(defaultVolume, time * 0.5f));
		}
		
		[Subscribe(AudioSignals.cmd_Stop_Music)]
		public void StopQuizMusic()
		{
			if (savedClip == null) {
				return;
			}

			if (savedClipWasPlaying)
				StartCoroutine (ResumeMusicImpl (savedClip, 4.0f));
			else {
				musicAudioSource.clip = savedClip;
			}
			musicAudioSource.time = savedTime;
			savedClipWasPlaying = false;
			savedClip = null;
		}

		[Subscribe (AudioSignals.cmd_Pause_Music)]
		public void MusicMenuPause (AudioPauseModel model=null)
		{
			string clipName = "";
			bool willStartFromZero = false;
			if (model != null) {
				clipName = model.pausedClipName;
				willStartFromZero = model.willStartFromZero;
			}

			currentMusicClip = musicAudioSource.clip;

			if (currentMusicClip == null) {
				return;
			}

			if (string.IsNullOrEmpty (clipName)) {
				GetLastPlayTime (willStartFromZero);
				musicAudioSource.Stop ();
				return;
			}

			if (!_repository.ContainsKey (clipName)) {
				return;
			}

			if (musicAudioSource.clip.name == _repository [clipName].name) {
				GetLastPlayTime (willStartFromZero);
				musicAudioSource.Stop ();
			}
		}

		[Subscribe(AudioSignals.cmd_Resume_Music)]
		public void ResumeMusic ()
		{
			if (currentMusicClip == null)
				return;
			StartCoroutine (ResumeMusicImpl (currentMusicClip, 4.0f));
		}

		private IEnumerator ResumeMusicImpl (AudioClip clipToResume, float durationToFullVolume)
		{
			yield return null;

			var defaultVolume = musicAudioSource.volume;
			musicAudioSource.clip = clipToResume;
			musicAudioSource.volume = 0;
			SetTime ();
			musicAudioSource.Play();
			while (musicAudioSource.volume < defaultVolume) {
				musicAudioSource.volume = Mathf.Min (musicAudioSource.volume + defaultVolume * Time.deltaTime / durationToFullVolume, defaultVolume);
				yield return waitForEndOfFrame;
			}
		}

		private void SetTime ()
		{
			//TODO: some way to skip it if chosen to always start from 0 (for victory music)
			if (_lastPlayTimes.ContainsKey (musicAudioSource.clip.name)) {
				var clip = musicAudioSource.clip;
				var t = _lastPlayTimes[clip.name];
				musicAudioSource.time = Mathf.Clamp(t, 0f, clip.length - 1f);
			} else {
				musicAudioSource.time = 0f;
			}
		}

		private void GetLastPlayTime (bool willStartFromZero)
		{
			if (musicAudioSource.clip == null)
				return;

			float lastPlayTime = willStartFromZero ? 0 : musicAudioSource.time;

			if (_lastPlayTimes.ContainsKey (musicAudioSource.clip.name)) {
				_lastPlayTimes [musicAudioSource.clip.name] = 
					Mathf.Min(lastPlayTime, musicAudioSource.clip.length - 0.01f);
				return;
			}

			if (!String.IsNullOrEmpty (musicAudioSource.clip.name)) {
				_lastPlayTimes.Add (musicAudioSource.clip.name, lastPlayTime);
			}
		}

		[Subscribe(AudioSignals.cmd_Listen_For_End_Of_Clip)]
		public void ClipQueue()
		{
			if(!contextAudioSource.isPlaying)
				bus.Publish(AudioSignals.cmd_Audio_Clip_Finished_Playing);
			else
				StartCoroutine(AudioClipFinishedNotifier());
		}

		[Subscribe (AudioSignals.cmd_Play_Audio_Clip)]
		public void ClipPlay (AudioClip clip)
		{
			if (clip == null)
				return;

			contextAudioSource.clip = clip;
			contextAudioSource.Play ();
		}

		[Subscribe (AudioSignals.cmd_Stop_Audio_Clip)]
		public void ClipStop ()
		{
			if (contextAudioSource == null)
				return;

			contextAudioSource.Stop ();
			contextAudioSource.clip = null;
		}
		
		[Subscribe (AudioSignals.cmd_Play_Audio_Sprite_Event)]
		public void AudioSpriteEventPlay (string eventName)
		{
			SpriteEvent spriteEvent;
			if (string.IsNullOrEmpty(eventName) || !_spriteEventRepository.TryGetValue(eventName, out spriteEvent)) {
				logger.Warning ("Sprite event was not registered on play event '{0}'", eventName);
				return;
			}
			
			if (!spriteEvent.audioClip) {
				logger.Warning ("AudioClip of Sprite Event not found at play event '{0}'", eventName);
				return;
			}

			var source = GetAudioSource ();
			source.clip = spriteEvent.audioClip; 
			source.Play ();
		}

		[Subscribe (AudioSignals.cmd_Stop_Audio_Sprite_Event)]
		public void AudioSpriteEventStop (string eventName)
		{
			SpriteEvent spriteEvent;
			if (string.IsNullOrEmpty(eventName) || !_spriteEventRepository.TryGetValue(eventName, out spriteEvent)) {
				logger.Warning ("Sprite event was not registered on stop event '{0}'", eventName);
				return;
			}
			
			if (!spriteEvent.audioClip) {
				logger.Warning ("AudioClip of Sprite Event not found at play event '{0}'", eventName);
				return;
			}

			for (var i = 0; i < audioSources.Length; i++) {
				var audioSource = audioSources[i];
				if (audioSource.clip == spriteEvent.audioClip) {
					audioSource.Stop ();
					break;
				}
			}
		}

		[Subscribe(AudioSignals.cmd_Play_AudioEvent_Sfx)]
		public void AudioEventSfxPlay(AudioEvent audioEvent)
		{
			if (audioEvent.clip == null) {
				if (string.IsNullOrEmpty (audioEvent.clipName) || !_repository.ContainsKey (audioEvent.clipName)) {
					logger.Warning ($"clip not found '{audioEvent.clipName}'");
					return;
				}
			}
			AudioSource source = GetAudioSource ();
			AudioClip clip = audioEvent.clip ? audioEvent.clip : _repository [audioEvent.clipName];
			if (clip == null)
				return;
			float defaultPitch = 1.0f;
			float defaultVolume = 0.5f;
			if (source == null) {
				oneShotAudioSource.pitch = defaultPitch * audioEvent.pitchFactor;
				oneShotAudioSource.volume = defaultVolume * audioEvent.volumeFactor;
				oneShotAudioSource.PlayOneShot(clip);
			} else {
				source.pitch = defaultPitch * audioEvent.pitchFactor;
				source.volume = defaultVolume * audioEvent.volumeFactor;
				source.clip = clip;
				source.Play();
			}
		}
		
		[Subscribe (AudioSignals.cmd_Play_Audio_Sfx)]
		public void AudioSfxPlay (string clipName)
		{
			if (string.IsNullOrEmpty (clipName) || !_repository.ContainsKey (clipName)) {
				logger.Warning ("clip not found '{0}'", clipName);
				return;
			}

			AudioSource source = GetAudioSource ();
			AudioClip clip = _repository [clipName];
			if (clip == null)
				return;
			if (source == null) {
				oneShotAudioSource.PlayOneShot(clip);
			} else {
				source.clip = clip;
				source.Play ();	
			}
		}

		[Subscribe (AudioSignals.cmd_Stop_Audio_Sfx)]
		public void AudioSfxStop (string clipName)
		{
			if (string.IsNullOrEmpty (clipName) || !_repository.ContainsKey (clipName)) {
				logger.Warning ("clip not found '{0}'", clipName);
				return;
			}

			foreach (var audioSource in audioSources) {
				if (audioSource.clip == _repository [clipName]) {
					audioSource.Stop ();
					break;
				}
			}
		}

		[Subscribe (AudioSignals.cmd_Play_Audio_Sfx_Random)]
		public void AudioFxPlayRandom (string[] clipNames)
		{
			//check if all clipnames are findable in the dictionary
			bool containsKey = false;
			for (int i = 0; i < clipNames.Length; i++) {
				var clipName = clipNames [i];
				containsKey = _repository.ContainsKey (clipName);
			}

			if (clipNames.Length == 0 || !containsKey) {
				logger.Error ("clip not found '{0}'", clipNames);
				return;
			}

			string name = clipNames [UnityEngine.Random.Range (0, clipNames.Length)];

			var source = GetAudioSource ();
			source.clip = _repository [name];
			source.Play ();
		}

		public AudioSource GetAudioSource (bool getNullIfNoneAvailable = false)
		{
			// find the first audio source that is not playing
			for (int i = 0; i < audioSources.Length; i++) {
				var source = audioSources [i];
				if (!source.isPlaying)
					return source;
			}

			// if we don't want to cancel the first AudioSource, return null
			if (getNullIfNoneAvailable)
				return null;

			// if nothing found, cancel the first one and use it.
			return audioSources [0];
		}

		void IAudioclipRepository.Register (AudioClipDataSource dataSource)
		{
			logger.Debug ("Registering data source: '{0}' of '{1}'", dataSource._name, dataSource.gameObject.name);
			var clips = dataSource.audioClips;
			if (clips == null)
				return;

			for (int i = 0; i < clips.Length; i++) {
				var item = clips [i];
//				logger.Debug ("clip: '{0}'", item.name);
				_repository [item.name] = item.clip;
			}
		}

		void IAudioclipRepository.UnRegister (AudioClipDataSource dataSource)
		{
			logger.Debug ("UnRegistering data source: '{0}' of '{1}'", dataSource._name, dataSource.gameObject.name);
			var clips = dataSource.audioClips;
			if (clips == null)
				return;

			for (int i = 0; i < clips.Length; i++) {
				_repository.Remove (clips [i].name);
			}
		}
		
		void IAudioclipRepository.Register (AudioSpriteDataSource dataSource)
		{
			logger.Debug ("Registering sprite data source: '{0}' of '{1}'", dataSource._name, dataSource.gameObject.name);
			var clip = dataSource.audioSprite;
			if (clip == null)
				return;
			_repository [clip.name] = clip;

			if (dataSource.spriteConfig == null || dataSource.spriteConfig.sprite == null) return;
			for (var i = 0; i < dataSource.spriteConfig.sprite.Length; i++) {
				var spriteEvent = dataSource.spriteConfig.sprite[i];
				_spriteEventRepository[spriteEvent.eventname] = spriteEvent;
			}
		}
		
		void IAudioclipRepository.UnRegister (AudioSpriteDataSource dataSource)
		{
			logger.Debug ("UnRegistering sprite source: '{0}' of '{1}'", dataSource._name, dataSource.gameObject.name);
			var clip = dataSource.audioSprite;
			if (clip == null)
				return;
			_repository.Remove (clip.name);

			var spriteEvents = dataSource.spriteConfig.sprite;
			foreach(var spriteEvent in spriteEvents) {
				_spriteEventRepository.Remove(spriteEvent.eventname);
			}
		}

		void IAudioclipRepository.Register (string clipName, AudioClip clip)
		{
			if (!_repository.ContainsKey (clipName))
				_repository.Add (clipName, clip);
		}

		void IAudioclipRepository.UnRegister (string clipName)
		{
			_repository.Remove (clipName);
		}

		IEnumerator AudioClipFinishedNotifier()
		{
			while (contextAudioSource.isPlaying)
				yield return waitForEndOfFrame;

			yield return waitSecond; // This is just so the next audio doesn't play immediately.
			bus.Publish(AudioSignals.cmd_Audio_Clip_Finished_Playing);
		}

		[Subscribe (AudioSignals.cmd_Audio_Set_Music_Volume)]
		public void changeMusicVolume(float val)
		{
			musicMixer.SetFloat("MusicVol", musicMuted ? -80f : Mathf.Log(val)*20);
			PlayerPrefs.SetFloat("musicVolume", val);
		}

		[Subscribe (AudioSignals.cmd_Audio_Set_Sound_Effect_Volume)]
		public void changeSoundVolume(float val)
		{
			soundMixer.SetFloat("SoundVol", Mathf.Log(val)*20);
			PlayerPrefs.SetFloat("soundVolume", val);
		}

		[Subscribe (AudioSignals.cmd_Audio_Set_Music_Mute)]
		public void changeMusicMute(bool val)
		{
			musicMuted = val;
			changeMusicVolume(PlayerPrefs.GetFloat("musicVolume"));
		}

	/*	private float scaleVolume(float val)
		{

			Debug.Log("AAAA> " + Mathf.Log(val));
			return (Mathf.Log(val, 20f));
		}*/
	}

}