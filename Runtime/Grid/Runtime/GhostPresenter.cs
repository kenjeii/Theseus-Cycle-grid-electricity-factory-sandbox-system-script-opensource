using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>
    /// Builds and drives a non-interactive preview instance of a placeable prefab.
    /// Physics and gameplay behaviours are stripped on creation only; per-frame calls are allocation-free.
    /// </summary>
    public sealed class GhostPresenter
    {
        private readonly Transform _parent;
        private readonly int _sortingOrder;
        private GameObject _instance;
        private SpriteRenderer[] _renderers;

        /// <summary>Creates a presenter that parents ghosts to the given transform.</summary>
        public GhostPresenter(Transform parent, int sortingOrder)
        {
            _parent = parent;
            _sortingOrder = sortingOrder;
        }

        /// <summary>Recreates the ghost from a prefab.</summary>
        public void Show(GameObject prefab)
        {
            Hide();
            if (prefab == null) return;

            _instance = Object.Instantiate(prefab, _parent);
            _instance.name = "[GHOST] " + prefab.name;

            Collider2D[] colliders = _instance.GetComponentsInChildren<Collider2D>(true);
            for (int i = 0; i < colliders.Length; i++) Object.Destroy(colliders[i]);

            Rigidbody2D[] bodies = _instance.GetComponentsInChildren<Rigidbody2D>(true);
            for (int i = 0; i < bodies.Length; i++) Object.Destroy(bodies[i]);

            Behaviour[] behaviours = _instance.GetComponentsInChildren<Behaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is PlaceableObject) continue;
                behaviours[i].enabled = false;
            }

            _renderers = _instance.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < _renderers.Length; i++) _renderers[i].sortingOrder = _sortingOrder;
        }

        /// <summary>Destroys the current ghost.</summary>
        public void Hide()
        {
            if (_instance == null) return;
            Object.Destroy(_instance);
            _instance = null;
            _renderers = null;
        }

        /// <summary>Moves the ghost to a world position.</summary>
        public void MoveTo(Vector3 world)
        {
            if (_instance == null) return;
            _instance.transform.position = world;
            _instance.transform.rotation = Quaternion.identity;
        }

        /// <summary>Tints all sprite renderers of the ghost.</summary>
        public void Tint(Color color)
        {
            if (_renderers == null) return;
            for (int i = 0; i < _renderers.Length; i++) _renderers[i].color = color;
        }
    }
}
