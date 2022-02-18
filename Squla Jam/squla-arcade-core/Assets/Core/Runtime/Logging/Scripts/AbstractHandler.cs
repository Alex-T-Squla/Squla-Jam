using System;

namespace Squla.Core.Logging
{
	public abstract class AbstractHandler
	{
		public ILogMeta logMetaData { get; set; } 
		public abstract void Log (string name, LogLevel level, string msg);
	}
}
