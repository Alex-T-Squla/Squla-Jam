using DG.Tweening;
using Squla.Core.IOC;
using Squla.Core.Network;
using UnityEngine;
using UnityEngine.UI;

namespace Squla.Core
{
	[RequireComponent(typeof(Image))]
	public class ContentImage: MonoBehaviourV2
	{
		[SerializeField] private Image image;
		[SerializeField] private Sprite defaultSprite;

		[SerializeField]
		[CacheNameDropdown]
		private string cacheName = CacheNames.Default;

		private IImageDownloader downloader;

		private string model;

		private GameObject _gs;

		private Tween fadeTween;

		private Color color = Color.white;

		public System.Action<Sprite> OnSpriteSet { get; set; }

		public Image Image {
			get => image;
			set => image = value;
		}

		protected override void AfterAwake()
		{
			_gs = gameObject;
		}

		public void ApplyModel(string imageUrl)
		{
			if (model != imageUrl) {
				// release the old one
				downloader?.ReleaseImage (model);
			} else {
				// Ignore since we are applying the same image already
				return;
			}

			model = imageUrl;
			ApplyModel();
		}

		public void SetColor(Color color)
		{
			if (fadeTween != null && fadeTween.IsActive() && fadeTween.IsPlaying()) {
				fadeTween.Kill(true);
			}
			this.color = color;
			if (Image.sprite != defaultSprite) {
				Image.color = color;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			ApplyModel();
		}

		protected override void OnDisable()
		{
			if (fadeTween != null && fadeTween.IsActive() && fadeTween.IsPlaying()) {
				fadeTween.Kill(true);
			}
			downloader?.ReleaseImage (model);
			model = null;
			base.OnDisable ();
		}

		private void ApplyModel()
		{
			if (string.IsNullOrEmpty(model)) {
				return;
			}

			Image.sprite = defaultSprite;
			Image.color = Color.white;
			if (downloader == null) {
				downloader = graph.Get<IImageDownloader>(cacheName);
			}
			
			downloader.GetImage(model, _gs, OnSpriteData);
		}

		private void OnSpriteData(string url, Sprite sprite)
		{
			if (!sprite || url != model)
				return;

			if (fadeTween != null && fadeTween.IsActive() && fadeTween.IsPlaying()) {
				fadeTween.Kill();
			}
			
			Image.sprite = sprite;
			var c = color;
			c.a = 0f;
			Image.color = c;

			fadeTween = Image.DOFade (color.a, 0.1f);

			OnSpriteSet?.Invoke(sprite);
		}

		#if UNITY_EDITOR

		private void OnValidate()
		{
			if (!Image) {
				Image = GetComponent<Image>();
			}

			if (image && defaultSprite) {
				image.sprite = defaultSprite;
			}
		}

		#endif
	}
}