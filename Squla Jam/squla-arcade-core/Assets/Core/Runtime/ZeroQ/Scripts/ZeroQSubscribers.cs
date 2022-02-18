using System;
using System.Reflection;
using System.Collections.Generic;
using Squla.Core.Logging;
using System.Linq;


namespace Squla.Core.ZeroQ
{
	internal class ZeroQSubscribers
	{
		private System.Object target;
		private Dictionary<string, SubscriberMeta> subscribers;
		private string[] commands;

		internal ZeroQSubscribers (System.Object target, Dictionary<string, SubscriberMeta> subscribers)
		{
			this.target = target;
			this.subscribers = subscribers;
			commands = subscribers.Keys.ToArray ();
		}

		public void Subscribe (Bus bus)
		{
			for (int i = 0; i < commands.Length; i++) {
				bus.GetSubscription (commands [i]).Subscribe (this);
			}
		}

		public void UnSubscribe (Bus bus)
		{
			for (int i = 0; i < commands.Length; i++) {
				bus.GetSubscription (commands [i]).UnSubscribe (this);
			}
		}

		public void Dispatch (string command, System.Object source)
		{
			var methodWrapper = subscribers [command];
			methodWrapper.Invoke (target, source);
		}
	}
}