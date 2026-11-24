using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>
    /// Abstracts instantiation so the placement pipeline can use Instantiate/Destroy,
    /// a prefab pool, or any custom lifetime strategy without changing callers.
    /// </summary>
    public interface IPlaceableActivator
    {
        /// <summary>Returns an active instance for the given prefab.</summary>
        GameObject Acquire(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent);

        /// <summary>Returns an instance to its provider.</summary>
        void Release(GameObject instance);
    }
}
