using System;

namespace Squla.Core.Network
{
	public class WebSocketAction
	{
		public string node_id;
		public string sub;
		public string unsub;

		public override string ToString ()
		{
			return string.Format ("node_id={0}, sub={1}, unsub={2}", node_id, sub, unsub);
		}
	}
}