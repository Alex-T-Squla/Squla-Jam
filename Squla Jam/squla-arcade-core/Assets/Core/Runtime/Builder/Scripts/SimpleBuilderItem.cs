using System;
using UnityEngine;

namespace Squla.Core.IOC.Builder
{
    [Serializable]
    public class SimpleBuilderItem
    {
        public string name;
        public string prefabName;
        public GameObject prefab;
        public RectTransform target;

        [HideInInspector] public GameObject instance;
    }
}