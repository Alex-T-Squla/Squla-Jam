using System;
using System.Reflection;
using System.Collections.Generic;
using Squla.Core.Logging;
using System.Linq;

namespace Squla.Core.ZeroQ
{
	public class Bus
	{
		static SmartLogger logger = SmartLogger.GetLogger<Bus> ();

		private Dictionary<Type, ZeroQMeta> subscribersMeta = new Dictionary<Type, ZeroQMeta> ();

		private Dictionary<System.Object, ZeroQSubscribers> subscribers = new Dictionary<System.Object, ZeroQSubscribers> ();

		private Dictionary<string, ZeroQSubscription> subscriptions = new Dictionary<string, ZeroQSubscription> ();

		private IBusDelegate busDelegate;

		public Bus (IBusDelegate busDelegate)
		{
			this.busDelegate = busDelegate;
		}

		public void DelayedRegister (System.Object target)
		{
			busDelegate.DelayedRegister (target);
		}

		public void Register (System.Object target)
		{
			var targetType = target.GetType ();

			if (!subscribersMeta.ContainsKey (targetType)) {
				subscribersMeta [targetType] = new ZeroQMeta (targetType);
			}

			var meta = subscribersMeta [targetType];

			if (meta.subscribersMeta != null) {
				if (!subscribers.ContainsKey (target)) {
					subscribers [target] = new ZeroQSubscribers (target, meta.subscribersMeta);
					subscribers [target].Subscribe (this);  // by this a target can only one time subscribe.
				}
			}
		}

		public void UnRegister (System.Object target)
		{
			if (subscribers.ContainsKey (target)) {
				subscribers [target].UnSubscribe (this);
				subscribers.Remove (target);
			}
		}

		public void DelayedPublish (string command)
		{
			logger.Debug ("DelayedPublish: {0}", command);
			busDelegate.DelayedPublish (command, null);
		}

		public void DelayedPublish (string command, System.Object source)
		{
			logger.Debug ("DelayedPublish: {0} {1}", command, source);
			busDelegate.DelayedPublish (command, source);
		}

		public void Publish (string command)
		{
			Publish (command, null);
		}

		public void Publish (string command, System.Object source)
		{
			logger.Debug ("Publish: {0} {1}", command, source);

			if (!ZeroQSubscription.IsPersistence (command) &&
			    (!subscriptions.ContainsKey (command) || subscriptions [command].SubscribersCount == 0)) {
				logger.Info (string.Format ("No active subscriber(s) for command '{0}'", command));
//				busDelegate.Notify(command, source);
				return;
			}

			GetSubscription (command);
			subscriptions [command].Dispatch (source);
		}

		internal ZeroQSubscription GetSubscription (string command)
		{
			if (!subscriptions.ContainsKey (command)) {
				subscriptions [command] = new ZeroQSubscription (command, busDelegate);
			}

			return subscriptions [command];
		}
	}

}
