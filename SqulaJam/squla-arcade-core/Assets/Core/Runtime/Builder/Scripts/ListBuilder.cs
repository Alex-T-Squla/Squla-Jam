using System;
using System.Collections.Generic;
using Squla.Core.Logging;
using Squla.Core.ZeroQ;
using WebSocketSharp;
using UnityEngine;

namespace Squla.Core.IOC.Builder
{
	public class ListBuilder : MonoBehaviour
	{
		static readonly SmartLogger logger = SmartLogger.GetLogger<ListBuilder> ();
		[Inject] private IPrefabProvider prefabProvider;
		public string _name;
		[Tooltip ("The name that will be given to the child GameObject created.")]
		public string childName;
		public GameObject prefab;
		public string prefabName;
		public RectTransform target;
		public string targetName;

		public event Action<RectTransform> OnChildAdded;
		public event Action<RectTransform, int> OnChildRemoved;
		public event Action OnChildrenCleared;

		public int ChildCount => _children.Count;
		public int Ensured { get; set; }
		
		private readonly List<RectTransform> _children = new List<RectTransform> ();
		public RectTransform _target { 
			get; 
			private set;
		}

		private ObjectGraph graph;
		private int index;
		
		void Awake()
		{
			graph = ObjectGraph.main;
			if (graph == null) {
				logger.Error("ObjectGraph is not initialized.");
				return;
			}

			graph.Resolve(this);

			var bus = graph.Get<Bus>();
			bus.Register(this);

			Build();
		}

		public void Rebuild()
		{
			Build();
		}
		
		private void Build()
		{
			if (!string.IsNullOrEmpty (targetName) && target != null) {
				var msg = $"target and targetName is mutually exclusive '{gameObject.name}'";
				throw new IOCException (msg);
			}

			if (!string.IsNullOrEmpty(prefabName)) {
				prefab = prefabProvider[prefabName];
			}
			
			_target = target;
			if (!string.IsNullOrEmpty (targetName)) {
				_target = graph.GetOrDefault<RectTransform> (targetName);
				if (_target == null) {
					var msg = $"Slot '{targetName}' was not provided by any provider";
					throw new IOCException (msg);
				}
			}
		}

		public T Resolve<T> (string overwriteName = "")
		{
			var instance = Instantiate (prefab);
			if (!string.IsNullOrEmpty (childName)) {
				instance.name = childName;
			}else if (!overwriteName.IsNullOrEmpty()) {
				instance.name = overwriteName;
			}
			var child = instance.GetComponent<RectTransform> ();
			child.SetParent (_target, false);
			_children.Add (child);
			Ensured = Math.Max(Ensured, _children.Count);
			OnChildAdded?.Invoke(child);
			return child.GetComponent<T> ();
		}

		public T Resolve<T> (int index, string name = "")
		{
			if (index < _children.Count) {
				var child = _children [index];
				child.gameObject.SetActive (true);
				return child.GetComponent<T> ();
			}
			var resp = Resolve <T>(name.IsNullOrEmpty() ? "" : name);
			return resp;
		}

		public RectTransform this[int i] {
			get {
				if (i < 0 || _children == null ||  i >= _children.Count)
					return null;
				return _children[i];
			}
		}

		public void Ensure (int count)
		{
			Ensured = count;
			for (int i = count; i < _children.Count; i++) {
				var child = _children [i];
				child.gameObject.SetActive (false);
				OnChildRemoved?.Invoke(child, i);
			}
		}

		/// <summary>
		/// This is temp only. not recommended to use.
		/// </summary>
		public void RemoveAll ()
		{
			for (int i = 0; i < _children.Count; i++) {
				var child = _children [i];
				Destroy (child.gameObject);
			}
			_children.Clear ();
			OnChildrenCleared?.Invoke();
		}

		public void RemoveLast ()
		{
			if (_children.Count == 0)
				return;

			index = _children.Count - 1;
			var child = _children [index];
			OnChildRemoved?.Invoke(_children[index], index);
			_children.RemoveAt (index);
			DestroyImmediate (child.gameObject);
		}
	}
}
