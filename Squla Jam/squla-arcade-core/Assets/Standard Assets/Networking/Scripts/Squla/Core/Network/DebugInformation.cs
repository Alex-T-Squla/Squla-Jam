using System;

namespace Squla
{
	public class DebugInformation
	{
		private static Random r = new Random ();

		public static string addDebugInformation (string url, string marker)
		{
			string sep = "?";

			if (url.IndexOf ("?") > 0) {
				sep = "&";
			} 

			return url + sep + marker + "=" + r.Next (0, 5000);
		}

	}
}

