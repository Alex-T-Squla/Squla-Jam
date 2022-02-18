public static class AudioSignals
{
	public const string cmd_Play_Music = "cmd://music/play";
	public const string cmd_Play_Temporary_Music = "cmd://music/playtemporary";
	public const string cmd_Pause_Music = "cmd://music/pause";
	public const string cmd_Resume_Music = "cmd://music/resume";
	public const string cmd_Switch_Music = "cmd://music/switch";
	public const string cmd_Stop_Music = "cmd://music/stop";
	public const string cmd_Back_To_Menu_Music = "cmd://music/backtomenumusic";
	
	public const string cmd_Play_Audio_Clip = "cmd://audio-clip/play";
	public const string cmd_Play_Audio_Sfx = "cmd://audio-sfx/play";
	public const string cmd_Play_AudioEvent_Sfx = "cmd://audio-sfx/audioevent/play";
	public const string cmd_Stop_Audio_Sfx = "cmd://audio-sfx/stop";
	public const string cmd_Play_Audio_Sfx_Random = "cmd://audio-sfx/play/random";
	public const string cmd_Play_Audio_Sprite_Event = "cmd://audio-sprite/play/event";
	public const string cmd_Stop_Audio_Sprite_Event = "cmd://audio-sprite/stop/event";

	public const string cmd_Audio_Set_Music_Volume = "cmd://audio/set/music/volume";
	public const string cmd_Audio_Set_Music_Mute = "cmd://audio/set/music/mute";
	public const string cmd_Audio_Set_Sound_Effect_Volume = "cmd://audio/set/sound/volume";

	public const string cmd_Stop_Audio_Clip = "cmd://audio-clip/stop";

	//	public const string PAUSE_AUDIO_SFX = "cmd://audio-fx/pause";

	public const string cmd_Listen_For_End_Of_Clip = "cmd://audio-clip/listen-for-end-of-clip";
	public const string cmd_Audio_Clip_Finished_Playing = "cmd://audio-clip/audio-clip-finished-playing";

	public const string CLIP_MUSIC_MENU = "sfx://music/menu";
	public const string CLIP_MISSION_VICTORY = "sfx://mission/end-victory";
	public const string CLIP_QUESTION_PANEL_ANSWER_ON_APPEAR = "sfx://question-panel/answer/on-appear";
	public const string CLIP_MUSIC_BOSSANOVA = "sfx://music/bossanova";
}

public class AudioSwitchModel
{
	public string currentClipName;
	public string nextClipName;
	public float time;
}

public class AudioPauseModel
{
	public string pausedClipName;
	public bool willStartFromZero;
}