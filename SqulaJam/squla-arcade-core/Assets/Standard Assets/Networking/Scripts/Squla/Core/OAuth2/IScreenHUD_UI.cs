using SimpleJson;

namespace Squla.Core.Network
{
	public interface IScreenHUD_UI
	{
		void SetIsFirstEnabled (bool isFirstEnabled);

		void SetIsSecondEnabled (bool isSecondEnabled);

		void StartTransition ();

		void EndTransition ();

		void ChangeToErroredCancel();
		
		void ChangeToErrored (int resCode, JsonObject resObject);

		void ShowError (string message = null, string first_command = null, string second_command = null);

		event System.Action Retry;
		event System.Action Done;
		event System.Action Abort;
	}
}
