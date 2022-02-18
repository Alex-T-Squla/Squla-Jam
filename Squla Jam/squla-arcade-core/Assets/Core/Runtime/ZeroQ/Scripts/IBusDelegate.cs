
namespace Squla.Core.ZeroQ
{
	public interface IBusDelegate
	{
		void DelayedPublish (string command, System.Object source);

		void DelayedRegister (System.Object target);

		void Notify(string command, System.Object source);
	}
}

