using System.Collections.Generic;
using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>Serialisable id-to-prefab mapping entry.</summary>
    [System.Serializable]
    public sealed class PlaceablePrefabEntry
    {
        /// <summary>Key used by the placement controller.</summary>
        public string id;

        /// <summary>Prefab that must contain a <see cref="PlaceableObject"/>.</summary>
        public GameObject prefab;
    }

    /// <summary>
    /// Creates placeables through an <see cref="IPlaceableActivator"/>, so a pool can replace
    /// Instantiate/Destroy without touching the placement pipeline.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlaceableFactory : MonoBehaviour
    {
        [SerializeField] private List<PlaceablePrefabEntry> _entries = new List<PlaceablePrefabEntry>();

        private readonly Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();
        private IPlaceableActivator _activator;
        private bool _indexed;

        private void Awake()
        {
            if (_activator == null) _activator = new DefaultPlaceableActivator();
            RebuildIndex();
        }

        /// <summary>Replaces the lifetime strategy.</summary>
        public void Initialize(IPlaceableActivator activator)
        {
            _activator = activator ?? new DefaultPlaceableActivator();
            RebuildIndex();
            _indexed = true;
        }

        private void EnsureIndex()
        {
            if (_indexed) return;
            RebuildIndex();
            _indexed = true;
        }

        /// <summary>Current lifetime strategy.</summary>
        public IPlaceableActivator Activator => _activator;

        /// <summary>Rebuilds the id lookup from the serialised entries.</summary>
        public void RebuildIndex()
        {
            _prefabs.Clear();
            for (int i = 0; i < _entries.Count; i++)
            {
                PlaceablePrefabEntry entry = _entries[i];
                if (entry == null || string.IsNullOrEmpty(entry.id) || entry.prefab == null) continue;
                _prefabs[entry.id] = entry.prefab;
            }
            _indexed = true;
        }

        /// <summary>Looks up a prefab by id.</summary>
        public bool TryGetPrefab(string id, out GameObject prefab)
        {
            EnsureIndex();
            return _prefabs.TryGetValue(id ?? string.Empty, out prefab);
        }

        /// <summary>Acquires an instance and returns its PlaceableObject component.</summary>
        public bool TryCreate(
            string id,
            Vector3 position,
            Quaternion rotation,
            Transform parent,
            out PlaceableObject instance)
        {
            instance = null;
            EnsureIndex();

            GameObject prefab;
            if (!_prefabs.TryGetValue(id ?? string.Empty, out prefab) || prefab == null) return false;

            GameObject instanceObject = _activator.Acquire(prefab, position, rotation, parent);
            if (instanceObject == null) return false;

            instance = instanceObject.GetComponent<PlaceableObject>();
            if (instance == null)
            {
                _activator.Release(instanceObject);
                return false;
            }
            return true;
        }

        /// <summary>Returns an instance to the current activator.</summary>
        public void Release(PlaceableObject instance)
        {
            if (instance == null) return;
            _activator.Release(instance.gameObject);
        }
    }
}
