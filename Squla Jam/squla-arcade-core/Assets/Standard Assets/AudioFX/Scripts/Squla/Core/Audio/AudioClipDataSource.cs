using Squla.Core.IOC;

namespace Squla.Core.Audio
{
	public class AudioClipDataSource : MonoBehaviourV2
	{
		public string _name;

		public AudioClipItem[] audioClips;

		[Inject]
		private IAudioclipRepository repository;

		protected override void AfterAwake ()
		{
			logger.Debug ("Working with: {0}", repository);
			// this not null check is for editor to not to report.
			if (repository != null) {
				repository.Register (this);
			}
		}

		void OnDestroy ()
		{
			// this not null check is for editor to not to report.
			if (repository != null) {
				repository.UnRegister (this);
			}
		}
	}
}
