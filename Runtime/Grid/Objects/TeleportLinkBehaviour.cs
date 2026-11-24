using System.Collections.Generic;
using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>
    /// Grid behaviour for teleport structures: channel-based nearest-partner pairing with
    /// re-entry cooldowns and layer filtering. No Animator or game manager dependency.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TeleportLinkBehaviour : MonoBehaviour, IGridBehaviour
    {
        private static readonly HashSet<TeleportLinkBehaviour> Registry = new HashSet<TeleportLinkBehaviour>();
        private static readonly Dictionary<GameObject, float> LastTeleportTime = new Dictionary<GameObject, float>();
        private static readonly Dictionary<GameObject, float> LastStayTriggerTime = new Dictionary<GameObject, float>();

        [SerializeField] private int _channel;
        [SerializeField] private float _searchRadius = 12f;
        [SerializeField] private Vector2 _arrivalOffset;
        [SerializeField] private bool _isPreview;
        [SerializeField] private LayerMask _teleportableLayers = ~0;
        [SerializeField] private LayerMask _excludeLayersFromSearch;
        [SerializeField] private float _reentryBlockTime = 0.25f;
        [SerializeField] private bool _triggerOnFirstEntry = true;
        [SerializeField] private bool _allowRetriggerWhileInside = true;
        [SerializeField] private float _retriggerInterval = 0.5f;

        private readonly HashSet<Collider2D> _inside = new HashSet<Collider2D>();
        private bool _busy;

        /// <inheritdoc />
        public void OnPlaced(IPlaceable placeable, GridPlacementService service)
        {
            Registry.Add(this);
            _busy = false;
        }

        /// <inheritdoc />
        public void OnRemoved()
        {
            Registry.Remove(this);
            _inside.Clear();
            _busy = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsEligible(other)) return;
            _inside.Add(other);
            if (_triggerOnFirstEntry) TryTeleport();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            _inside.Remove(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!_allowRetriggerWhileInside || _busy || !IsEligible(other)) return;

            GameObject root = GetRoot(other);
            float last;
            if (LastStayTriggerTime.TryGetValue(root, out last) && Time.time - last < _retriggerInterval) return;

            LastStayTriggerTime[root] = Time.time;
            _inside.Add(other);
            TryTeleport();
        }

        /// <summary>Attempts one teleport of all eligible overlapping objects.</summary>
        public bool TryTeleport()
        {
            if (_busy) return false;

            TeleportLinkBehaviour target = FindNearestPartner();
            if (target == null) return false;

            HashSet<GameObject> roots = new HashSet<GameObject>();
            foreach (Collider2D col in _inside)
            {
                if (col == null || !IsEligible(col)) continue;
                roots.Add(GetRoot(col));
            }
            if (roots.Count == 0) return false;

            _busy = true;
            Vector3 destination = target.transform.position + (Vector3)_arrivalOffset;

            foreach (GameObject root in roots)
            {
                if (root == null) continue;
                LastTeleportTime[root] = Time.time;

                Vector3 next = new Vector3(destination.x, destination.y, root.transform.position.z);
                Rigidbody2D body = root.GetComponent<Rigidbody2D>();
                if (body != null) body.position = new Vector2(next.x, next.y);
                else root.transform.position = next;
            }

            _busy = false;
            return true;
        }

        private bool IsEligible(Collider2D col)
        {
            if (col == null) return false;
            if ((_teleportableLayers.value & (1 << col.gameObject.layer)) == 0) return false;

            GameObject root = GetRoot(col);
            float last;
            if (LastTeleportTime.TryGetValue(root, out last) && Time.time - last < _reentryBlockTime) return false;
            return true;
        }

        private TeleportLinkBehaviour FindNearestPartner()
        {
            TeleportLinkBehaviour best = null;
            float bestSquared = float.PositiveInfinity;
            Vector3 origin = transform.position;
            float radiusSquared = _searchRadius * _searchRadius;

            foreach (TeleportLinkBehaviour candidate in Registry)
            {
                if (candidate == null || ReferenceEquals(candidate, this)) continue;
                if (!candidate.isActiveAndEnabled || candidate._isPreview) continue;
                if (candidate._channel != _channel) continue;
                if ((_excludeLayersFromSearch.value & (1 << candidate.gameObject.layer)) != 0) continue;

                float squared = (candidate.transform.position - origin).sqrMagnitude;
                if (squared > radiusSquared || squared >= bestSquared) continue;

                bestSquared = squared;
                best = candidate;
            }
            return best;
        }

        private static GameObject GetRoot(Collider2D col)
        {
            if (col.attachedRigidbody != null) return col.attachedRigidbody.gameObject;
            return col.transform.root.gameObject;
        }
    }
}
