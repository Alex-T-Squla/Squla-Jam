using System;
using System.Collections.Generic;
using Squla.Core.Logging;
using Squla.Core.ZeroQ;
using UnityEngine;

namespace Squla.Core.IOC.Builder
{
    public class SimpleBuilder : MonoBehaviour
    {
        static readonly SmartLogger logger = SmartLogger.GetLogger<SimpleBuilder>();

        [Inject] private IPrefabProvider prefabProvider;

        [Tooltip("When this field is true, instantiated prefabs will receive the name specified in the item")]
        public bool assignName;

        public SimpleBuilderItem[] itemsToBuild;

        public LazyBuilderItem[] lazyItemsToBuild;

        private ObjectGraph graph;
        private Dictionary<string, GameObject> _namedObjects = new Dictionary<string, GameObject>();

#if UNITY_EDITOR
        public IPrefabProvider PrefabProvider {
            set => prefabProvider = value;
        }
#endif

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

        public void ResetItem(string nameObject)
        {
            SimpleBuilderItem item = null;

            for (int i = 0; i < itemsToBuild.Length; i++) {
                if (itemsToBuild[i].name == nameObject) {
                    item = itemsToBuild[i];
                    break;
                }
            }

            if (item == null) {
                return;
            }

            if (_namedObjects.TryGetValue(nameObject, out GameObject o)) {
                Destroy(o);
                _namedObjects.Remove(nameObject);
                BuildItem(item);
            }
        }

        private void BuildItem(SimpleBuilderItem item)
        {
            GameObject prefab = item.prefab;

            if (prefab == null && !string.IsNullOrEmpty(item.prefabName) && prefabProvider != null) {
                prefab = prefabProvider[item.prefabName];
            }

            if (prefab == null) {
                if (Application.isPlaying)
                    Debug.LogError($"Prefab in {gameObject.name} is null: {item.name}");
                return;
            }

            var instance = Instantiate(prefab);
            if (assignName) {
                instance.name = item.name;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying) {
                instance.name += "<Preview>";
                instance.tag = "EditorOnly";
                if (item.instance && instance.CompareTag("EditorOnly")) {
                    DestroyImmediate(item.instance);
                    _namedObjects.Remove(item.name);
                }
            }
#endif
            item.instance = instance;

            var t = instance.GetComponent<RectTransform>();
            if (!item.target && Application.isPlaying) {
                logger.Error($"Building item {prefab.name} from {gameObject.name} but target is null");
            }

            t.SetParent(item.target, false);
            _namedObjects.Add(item.name, instance);
        }

        public void Build()
        {
            for (int i = 0; itemsToBuild != null && i < itemsToBuild.Length; i++) {
                BuildItem(itemsToBuild[i]);
            }

            for (int i = 0; lazyItemsToBuild != null && i < lazyItemsToBuild.Length; i++) {
                var item = lazyItemsToBuild[i];

                if (item.instance) {
                    Debug.LogWarning("Found already existing item, deleting");
                    DestroyImmediate(item.instance);
                }
                
                var instance = Instantiate(item.prefab);
                if (assignName) {
                    instance.name = item.name;
                }

                item.instance = instance;
                var t = instance.GetComponent<RectTransform>();
                var target = graph.GetOrDefault<RectTransform>(item.targetName);
                if (target == null) {
                    string msg = $"Slot '{item.targetName}' was not provided by any provider";
                    throw new IOCException(msg);
                }

                t.SetParent(target, false);
                _namedObjects.Add(item.name, instance);
            }
        }

        public void ClearBuilder(bool immediate = false)
        {
            _namedObjects.Clear();
            for (int i = 0; itemsToBuild != null && i < itemsToBuild.Length; i++) {
                var item = itemsToBuild[i];
                if (immediate)
                    DestroyImmediate(item.instance);
                else {
                    Destroy(item.instance);
                }
            }

            for (int i = 0; lazyItemsToBuild != null && i < lazyItemsToBuild.Length; i++) {
                var item = lazyItemsToBuild[i];
                Destroy(item.instance);
            }
        }

        public Component Resolve(string name, Type type)
        {
            if (!_namedObjects.ContainsKey(name)) {
                string msg =
                    string.Format(
                        "Trying to resolve '{0}' for '{1}' where the name is not specified in SimpleBuilder instance",
                        name, gameObject.name);
                logger.Error(msg);
                throw new IOCException(msg);
            }

            return _namedObjects[name].GetComponent(type);
        }

        void OnDestroy()
        {
            // lazy items needs to be destroyed here.
            for (int i = 0; lazyItemsToBuild != null && i < lazyItemsToBuild.Length; i++) {
                var item = lazyItemsToBuild[i];
                Destroy(_namedObjects[item.name]);
            }
        }
    }
}