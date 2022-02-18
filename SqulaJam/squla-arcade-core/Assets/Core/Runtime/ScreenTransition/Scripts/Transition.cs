using DG.Tweening;
using UnityEngine;

namespace Squla.ScreenTransition
{
	public abstract class Transition : ScriptableObject
	{
		public abstract Tween TransitionIn(RectTransform rt);
		public abstract Tween TransitionOut(RectTransform rt);
		
		public const string NoTransition = "NoTransition";
		public const string WipeInFromTop = "WipeInFromTop";
		public const string WipeInFromRight = "WipeInFromRight";
		public const string WipeInFromBottom = "WipeInFromBottom";

		public const string WipeInEnd = "WipeInEnd";

		public const string WipeOutToTop = "WipeOutToTop";
		public const string WipeOutToRight = "WipeOutToRight";
		public const string WipeOutToBottom = "WipeOutToBottom";
		
		public static Tween NoTransitionIn(RectTransform rt)
		{
			return DOTween.Sequence()
				.AppendCallback(() => rt.anchoredPosition = Vector2.zero);
		}

		public static Tween NoTransitionOut(RectTransform rt)
		{
			return DOTween.Sequence()
				.AppendCallback(() => rt.anchoredPosition = Vector2.zero);
		}
	}
}
