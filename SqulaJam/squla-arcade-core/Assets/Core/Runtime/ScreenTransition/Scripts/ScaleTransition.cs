using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Squla.ScreenTransition
{
	[CreateAssetMenu(fileName = "Scale", menuName = "Squla/Data/Screen Transitions/Scale")]
	public class ScaleTransition : Transition
	{
		public enum EType
		{
			Center,
			ClickPosition
		}

		[SerializeField] private EType type;
		[SerializeField] private float time;
		[SerializeField] private float startScale;
		[SerializeField] private Ease scaleEasing;

		private Vector3 startPosition = Vector2.zero;
		public override Tween TransitionIn(RectTransform rt)
		{
			return DOTween.Sequence()
				.AppendCallback(() =>
				{
					rt.localScale = Vector3.one * startScale;
				})
				.Append(rt.DOScale(1f,time).SetEase(scaleEasing));
		}

		public override Tween TransitionOut(RectTransform rt)
		{
			return DOTween.Sequence()
				.AppendCallback(() => rt.localScale = Vector3.one * startScale)
				.Append(rt.DOScale(startScale,time).SetEase(scaleEasing));
		}
	}
}