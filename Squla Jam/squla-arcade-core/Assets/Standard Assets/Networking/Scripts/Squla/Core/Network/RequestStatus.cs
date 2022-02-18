using System;

namespace Squla.Core.Network
{
	public enum RequestStatus
	{
		Initial,
		NullAsset,
		Downloading,
		Queued,
		Ready,
		FileNotFound,
		Errored,
		Aborted

	}
}
