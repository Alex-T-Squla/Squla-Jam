using UnityEngine;

namespace Squla.Core.IOC.Builder
{
    public interface IPrefabProvider
    {
        GameObject this[string name] { get; }
    }
}