using DG.Tweening;
using Squla.Core.ZeroQ;
using Squla.ScreenTransition;
using UnityEngine;
using UnityEngine.UI;

namespace Squla.Core.IOC.Builder
{
	public class ScreenBuilder : MonoBehaviourV2
	{
		public const string cmd_Screen_Transition_In = "cmd://screen/transition/in";
		public const string cmd_Screen_Transition_Out = "cmd://screen/transition/out";

		[Inject] private IPrefabProvider prefabProvider;
		
		public string phonePrefab;
		public string tabletPrefab;
		
		[SerializeField] private Transition transition;
		[SerializeField] private bool segmented;

		private GameObject prefab;
		private GameObject screenInstance;
		private IVisibility visibleInstance;
		private RectTransform _rectTransform;
		private Tween transitionTween;

		public RectTransform RectTransform {
			get {
				if (_rectTransform == null) {
					_rectTransform = GetComponent<RectTransform> ();
				}

				return _rectTransform;
			}
		}

		private string _gameObjectName;

		public string GameObjectName {
			get {
				if (string.IsNullOrEmpty (_gameObjectName)) {
					_gameObjectName = gameObject.name;
				}
				return _gameObjectName;
			}
		}

		[Inject]
		private IScreenPrefabChooser sizeChooser;

	    protected override void AfterAwake ()
	    {
		    // Temporary while we change all of the rectTransform manually
		    RectTransform.anchorMax = Vector2.one;
		    RectTransform.anchorMin = Vector2.zero;
	    }

	    public ScreenBuilder Build ()
		{
			if (screenInstance == null) {
				LoadPrefab ();

				screenInstance = Instantiate (prefab);
				visibleInstance = screenInstance.GetComponent<IVisibility>();
				var screen = screenInstance.GetComponent<RectTransform> ();
				screen.SetParent (transform, false);

//				animator = _animator != null ? _animator : screenInstance.GetComponent<Animator> ();
			}

			screenInstance.SetActive (true);
			NotifyVisibility(true);
			return this;
		}

		public bool IsActive {
			get { return screenInstance != null && screenInstance.activeSelf; }
		}

		public T Resolve<T> ()
		{
			if (screenInstance == null) {
				throw new IOCException ("Resolve should be always called with Build().Resolve<T>()");
			}

			// NOTE: this is deliberately chosen to check the component
			// in the game object where screen builder is added to facilitate IWorkflowScreen
			var comp = GetComponent<T> ();
			if (comp != null)
				return comp;

			return screenInstance.GetComponent<T> ();
		}

		public ScreenBuilder DeactivateScreen ()
		{
			if (screenInstance != null) {
				screenInstance.SetActive(false);
				NotifyVisibility(false);
			}
			KillTransition();
			return this;
		}

		public void NotifyVisibility(bool visible)
		{
			if (visibleInstance != null)
				visibleInstance.OnVisibleChange(visible);
		}

		public void DestroyScreen ()
		{
			if (screenInstance != null) {
				NotifyVisibility(false);
				Destroy(screenInstance);
			}

			KillTransition ();
			_rectTransform = null;
			screenInstance = null;
			prefab = null;
		}

		public ScreenBuilder SetScreenActive (bool active)
		{
			if (screenInstance != null) {
				screenInstance.SetActive (active);
			}
			return this;
		}

		public ScreenBuilder Show(string command = null)
		{
			RectTransform.anchoredPosition = Vector2.zero;
			bus.Publish(string.IsNullOrEmpty(command)? cmd_Screen_Transition_In : command);
			return this;
		}
		
		public ScreenBuilder TransitionInModal()
		{
			logger.Debug($"Transitioning Modal into {screenInstance}");
			KillTransition();
			transitionTween = transition ? 
				transition.TransitionIn(RectTransform) :
				Transition.NoTransitionIn(RectTransform);
			return this;
		}

		public ScreenBuilder TransitionIn(string command = null, bool noTransition = false)
		{
			logger.Debug($"Transitioning into {screenInstance}");
			KillTransition();
			transitionTween = transition && !noTransition
				? transition.TransitionIn(RectTransform)
				: Transition.NoTransitionIn(RectTransform);
			transitionTween.onComplete = () => 
				bus.Publish(string.IsNullOrEmpty(command) ? cmd_Screen_Transition_In : command);
			return this;
		}

		public Sequence TransitionOut(string command = null)
		{
			logger.Debug($"Transitioning out of {screenInstance}");
			KillTransition();
			transitionTween = transition ?
				transition.TransitionOut(RectTransform) :
				Transition.NoTransitionOut(RectTransform);
			transitionTween.onComplete = () => 
				bus.Publish(string.IsNullOrEmpty(command) ? cmd_Screen_Transition_Out : command);
			return (Sequence)transitionTween;
		}
		
		public ScreenBuilder SetAsFirstSibling ()
		{
			RectTransform.SetAsFirstSibling ();
			return this;
		}

		public ScreenBuilder SetAsLastSibling ()
		{
			RectTransform.SetAsLastSibling ();
			return this;
		}

		[Subscribe("cmd://segmentation/switch")]
		public void SegmentationSwitched()
		{
			if (segmented)
				Unload();
		}

		public void Unload ()
		{
			DestroyScreen ();
			prefab = null;
		}

		public ScreenBuilder SetAnimatorDefault ()
		{
			return this;
		}

		public ScreenBuilder SetAnimatorWipeInEnd ()
		{
			return this;
		}

		public ScreenBuilder OverrideSortingLayer(string layerName)
		{
			var canvas = screenInstance.GetComponent<Canvas>();

			if (!canvas)
				canvas = screenInstance.AddComponent<Canvas>();

			if (!screenInstance.GetComponent<GraphicRaycaster>())
				screenInstance.AddComponent<GraphicRaycaster>(); // Leave defaults as they are

			canvas.overrideSorting = true;
			canvas.sortingLayerName = layerName;

			return this;
		}

		private void LoadPrefab ()
		{
			if (prefab != null)
				return;

			var desiredPrefab = (sizeChooser.WhichOne == ScreenSize.Size_3x4) ? tabletPrefab : phonePrefab;
			
			prefab = segmented ? prefabProvider[desiredPrefab] : Resources.Load<GameObject> (desiredPrefab);
		}

	    public void KillTransition ()
	    {
		    if (transitionTween != null && transitionTween.IsActive() && transitionTween.IsPlaying()) {
			    transitionTween.Kill(true);
		    }
	    }
	}
}
