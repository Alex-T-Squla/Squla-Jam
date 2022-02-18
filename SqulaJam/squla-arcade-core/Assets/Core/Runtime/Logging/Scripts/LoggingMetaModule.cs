using Squla.Core.IOC;

namespace Squla.Core.Logging
{
	[SingletonModule]
	public class LoggingMetaModule : MonoBehaviourV2 
	{
		protected override void AfterAwake()
		{
			graph.RegisterModule (this);
			var meta = graph.Get<ILogMeta>();
			SmartLogManager.SetLogMeta(meta);
		}

		[Provides]
		[Singleton]
		public ILogMeta provideLogMetaData(LogMetaData logMetaData)
		{
			return logMetaData;
		}
	}	
}
