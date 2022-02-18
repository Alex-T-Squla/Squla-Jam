using System.Collections.Generic;

namespace Squla.Core.ZeroQ
{
	internal class ZeroQSubscription
	{
		private const string kPersistenceProto = "model://";

		private readonly string command;

		private readonly List<ZeroQSubscribers> items = new List<ZeroQSubscribers> ();
		private readonly ProducedItem producedItem;
		private readonly bool supportPersistence;
		private readonly IBusDelegate busDelegate;

		public int SubscribersCount {
			get { return items.Count; }
		}

		public static bool IsPersistence (string command)
		{
			return  command.StartsWith (kPersistenceProto);
		}

		public ZeroQSubscription (string command, IBusDelegate busDelegate)
		{
			this.command = command;
			this.busDelegate = busDelegate;
			supportPersistence = IsPersistence (command);
			producedItem = supportPersistence ? new ProducedItem() : null;
		}

		public void Subscribe (ZeroQSubscribers item)
		{
			items.Add (item);

			// if producer is present for this subscription, then dispatch the produce
			if (supportPersistence && producedItem.is_valid) {
				item.Dispatch (command, producedItem.value);
			}
		}

		public void UnSubscribe (ZeroQSubscribers item)
		{
			items.Remove (item);
		}

		public void Dispatch (System.Object source)
		{
			if (supportPersistence) {
				producedItem.value = source;
				producedItem.is_valid = true;
			}

//			busDelegate.Notify(command, source);

			for (var i = 0; i < items.Count; i++) {
				items [i].Dispatch (command, source);
			}
		}
	}

	internal class ProducedItem
	{
		public System.Object value;
		public bool is_valid;
	}
}
