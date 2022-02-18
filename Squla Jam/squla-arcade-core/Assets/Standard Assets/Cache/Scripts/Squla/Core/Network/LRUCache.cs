using System.Collections.Generic;
using Squla.Core.ObjectPooling;

namespace Squla.Core.Network
{
	public class LRUCache<V> where V : class
	{
		public delegate void ItemDeleted (string key, LRUCache<V> cache);

		public event ItemDeleted OnItemDeleted;

		private readonly PoolManager<Node<V>> _pool;

		private readonly int _maxCapacity;
		private readonly Dictionary<string, Node<V>> _LRUCache;
		private readonly Node<V> _head;

		public LRUCache (int argMaxCapacity)
		{
			_maxCapacity = argMaxCapacity;
			_pool = new PoolManager<Node<V>>();
			_LRUCache = new Dictionary<string, Node<V>> ();
			_head = _pool.Resolve();
			_head.Next = _head.Previous = _head;
		}

		public void Insert (string key, V value)
		{
			if (_LRUCache.ContainsKey (key)) {
				MakeMostRecentlyUsed (_LRUCache [key]);
				return;
			}

			if (_LRUCache.Count >= _maxCapacity)
				RemoveUsed (_head.Previous);

			var insertedNode = _pool.Resolve();
			insertedNode.Data = value;
			insertedNode.Key = key;

			MakeMostRecentlyUsed (insertedNode);

			_LRUCache.Add (key, insertedNode);
		}

		public V GetItem (string key)
		{
			if (string.IsNullOrEmpty (key))
				return default(V);

			if (!_LRUCache.ContainsKey (key))
				return default(V);

			MakeMostRecentlyUsed (_LRUCache [key]);

			return _LRUCache [key].Data;
		}

		public V RemoveItem (string key)
		{
			if (string.IsNullOrEmpty (key))
				return default(V);

			if (!_LRUCache.ContainsKey (key))
				return default(V);

			var nodeToBeRemoved = _LRUCache [key];
			var value = nodeToBeRemoved.Data;
			RemoveUsed (nodeToBeRemoved);
			return value;
		}

		public int Size ()
		{
			return _LRUCache.Count;
		}

		private void RemoveUsed (Node<V> tail)
		{
			OnItemDeleted?.Invoke (tail.Key, this);
			_LRUCache.Remove (tail.Key);
			tail.Previous.Next = tail.Next;
			tail.Next.Previous = tail.Previous;
			_pool.Release(tail);
		}

		private void MakeMostRecentlyUsed (Node<V> foundItem)
		{
			if (foundItem == _head.Next)
				// this is already in the top of the list. so do nothing.
				return;

			// first remove if already part of the list
			if (foundItem.Next != null) {
				foundItem.Previous.Next = foundItem.Next;
				foundItem.Next.Previous = foundItem.Previous;
			}

			// then insert it at the beginning.
			foundItem.Next = _head.Next;
			foundItem.Previous = _head;
			_head.Next.Previous = foundItem;
			_head.Next = foundItem;
		}

		public void Clear ()
		{
			while (_LRUCache.Count > 0) {
				RemoveUsed(_head.Previous);
			}
		}

		private class Node<D>: IPoolableObject where D: class
		{
			public D Data { get; set; }

			public string Key { get; set; }

			public Node<D> Previous { get; set; }

			public Node<D> Next { get; set; }

			public Node()
			{
			}

			public void Release()
			{
				Previous = Next = null;
				Key = null;
				Data = null;
			}
		}
	}
}
