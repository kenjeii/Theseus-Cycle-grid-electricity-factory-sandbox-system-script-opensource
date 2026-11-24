using System.Collections.Generic;
using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>
    /// Grid behaviour for wire tiles: selects a sprite from the four-direction neighbour mask.
    /// Neighbour lookups are O(1) through a static registry.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class WireConnectableBehaviour : MonoBehaviour, IGridBehaviour
    {
        private static readonly Dictionary<Vector2Int, WireConnectableBehaviour> Registry =
            new Dictionary<Vector2Int, WireConnectableBehaviour>();

        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
        };

        [SerializeField] private WireSpriteSet _spriteSet;
        [SerializeField] private bool _trackMovement = true;

        private SpriteRenderer _renderer;
        private GridPlacementService _service;
        private Vector2Int _cell;
        private bool _registered;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (!_trackMovement || !_registered || _service == null) return;

            Vector2Int next = _service.Coordinates.WorldToGrid(transform.position);
            if (next == _cell) return;
            MoveTo(next);
        }

        /// <inheritdoc />
        public void OnPlaced(IPlaceable placeable, GridPlacementService service)
        {
            _service = service;
            _cell = service.Coordinates.WorldToGrid(transform.position);
            Registry[_cell] = this;
            _registered = true;
            Refresh();
            NotifyNeighbours(_cell);
        }

        /// <inheritdoc />
        public void OnRemoved()
        {
            if (!_registered) return;
            if (Registry.TryGetValue(_cell, out var owner) && ReferenceEquals(owner, this)) Registry.Remove(_cell);
            _registered = false;
            NotifyNeighbours(_cell);
        }

        private void MoveTo(Vector2Int next)
        {
            if (Registry.TryGetValue(_cell, out var owner) && ReferenceEquals(owner, this)) Registry.Remove(_cell);
            Vector2Int previous = _cell;
            _cell = next;
            Registry[_cell] = this;
            Refresh();
            NotifyNeighbours(previous);
            NotifyNeighbours(_cell);
        }

        private void Refresh()
        {
            if (_renderer == null || _spriteSet == null) return;

            int mask = 0;
            for (int i = 0; i < Directions.Length; i++)
            {
                if (Registry.ContainsKey(_cell + Directions[i])) mask |= 1 << i;
            }
            _renderer.sprite = _spriteSet.GetSprite(mask);
        }

        private void NotifyNeighbours(Vector2Int center)
        {
            for (int i = 0; i < Directions.Length; i++)
            {
                WireConnectableBehaviour neighbour;
                if (Registry.TryGetValue(center + Directions[i], out neighbour) && neighbour != null) neighbour.Refresh();
            }
        }
    }
}
