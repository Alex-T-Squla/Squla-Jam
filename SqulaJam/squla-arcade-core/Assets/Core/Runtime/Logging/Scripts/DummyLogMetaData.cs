namespace Squla.Core.Logging
{
	public class DummyLogMetaData : ILogMeta 
	{
		public string GetContext()
		{
			return string.Empty;
		}

		public void SetUserId(string user_id)
		{
			// Do nothing
		}
	}
}
