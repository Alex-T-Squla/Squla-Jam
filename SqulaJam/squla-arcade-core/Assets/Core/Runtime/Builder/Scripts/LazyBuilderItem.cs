using System;
using UnityEngine;

namespace Squla.Core.IOC.Builder
{
    [Serializable]
    public class LazyBuilderItem
    {
        public string name;
        public GameObject prefab;
        public string targetName;

        [HideInInspector] public GameObject instance;
    }
}