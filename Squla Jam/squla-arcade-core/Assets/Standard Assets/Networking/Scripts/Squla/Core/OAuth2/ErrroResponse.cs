
namespace Squla.Core.Network
{
	public class ErrorLite
	{
		public string type;
		public string message;
		public string action;
		public string action_cta;
	}

	public class ErrorResponse
	{
		public ErrorLite error;
	}
}

