using DG.Tweening;
using UnityEngine;

namespace Squla.ScreenTransition
{
	[CreateAssetMenu(fileName = "Slide", menuName = "Squla/Data/Screen Transitions/Wipe")]
	public class SlideTransition : Transition
	{
		public enum EDirection
		{
			None,
			Random,
			Top,
			Right,
			Left,
			Bottom
		}

		[SerializeField] private EDirection direction;
		[SerializeField] private float time;
		[SerializeField] private Ease easing;
		public override Tween TransitionIn(RectTransform rt)
		{
			rt.anchoredPosition = GetPosition(rt.rect);
			return DOTween.Sequence()
				.AppendCallback(() => rt.anchoredPosition = GetPosition(rt.rect))
				.Append(rt.DOAnchorPos(Vector2.zero, time).SetEase(easing));
		}

		public override Tween TransitionOut(RectTransform rt)
		{
			return DOTween.Sequence()
				.AppendCallback(() => rt.anchoredPosition = Vector2.zero)
				.Append(rt.DOAnchorPos(GetPosition(rt.rect), time).SetEase(easing));
		}
		
		private Vector2 GetPosition(Rect r)
		{
			if (direction == EDirection.None)
				return Vector2.zero;
			
			var d = direction;
			var v = Vector2.zero;
			
			if (d == EDirection.Random) {
				// Update this if you add more directions!
				d = (EDirection)Random.Range(2, 6);
			}

			switch (d) {
				case EDirection.Bottom:
					return new Vector2(0, -r.height);
				case EDirection.Top:
					return new Vector2(0, r.height);
				case EDirection.Right:
					return new Vector2(r.width, 0);
				case EDirection.Left:
					return new Vector2(-r.width, 0);
			}
			return Vector2.zero;
		}
	}
}