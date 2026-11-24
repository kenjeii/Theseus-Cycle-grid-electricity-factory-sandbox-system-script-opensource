using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>Default activator using Unity Instantiate and Destroy.</summary>
    public sealed class DefaultPlaceableActivator : IPlaceableActivator
    {
        /// <inheritdoc />
        public GameObject Acquire(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (prefab == null) return null;
            return Object.Instantiate(prefab, position, rotation, parent);
        }

        /// <inheritdoc />
        public void Release(GameObject instance)
        {
            if (instance != null) Object.Destroy(instance);
        }
    }
}
