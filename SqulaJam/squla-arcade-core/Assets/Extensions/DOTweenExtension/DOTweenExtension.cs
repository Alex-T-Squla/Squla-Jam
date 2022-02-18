using DG.Tweening;

public static class DOTweenExtension {

	public static bool SafeKill(this Tween tween, bool complete = false)
	{
		if (tween == null || !tween.IsActive() || !tween.IsPlaying()) return false;
		tween.Kill(complete);
		return true;
	}

	public static bool IsPlayingCheck(this Tween tween)
	{
		return !(tween == null || !tween.IsActive() || !tween.IsPlaying());
	}

}
