using Squla.Core.Network;
using Squla.Core.Logging;
using SimpleJson;
using Squla.Core.IOC;

namespace Squla.TDD
{
	public class ScreenHUDMock : IScreenHUD_UI
	{
		private readonly SmartLogger logger = SmartLogger.GetLogger<ScreenHUDMock>();

		public event System.Action Retry;
		public event System.Action Done;
		public event System.Action Abort;

		[Inject]
		public ScreenHUDMock()
		{
		}

		void IScreenHUD_UI.SetIsFirstEnabled (bool isFirstEnabled)
		{

		}

		void IScreenHUD_UI.SetIsSecondEnabled (bool isSecondEnabled)
		{

		}

		void IScreenHUD_UI.StartTransition ()
		{

		}

		void IScreenHUD_UI.EndTransition ()
		{

		}

		public void ChangeToErroredCancel()
		{
			
		}

		void IScreenHUD_UI.ChangeToErrored (int resCode, JsonObject resObj)
		{
			logger.Debug ("ChangeToErrored");
		}

		void IScreenHUD_UI.ShowError (string message, string first_command, string second_command)
		{
			logger.Debug ("ShowError: {0}", message);
		}

		public void RetryOperations ()
		{
			logger.Debug("Retry operations");
			Retry?.Invoke();
		}

		public void AbortOperations ()
		{
			logger.Debug("Abort operations");
			Abort?.Invoke();
		}
	}

}
