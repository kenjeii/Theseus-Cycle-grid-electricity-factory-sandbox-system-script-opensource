using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>
    /// Base component for every grid placeable. Carries footprint data, inventory metadata,
    /// optional destruction state and cached power-device references. Game-specific behaviour
    /// belongs in <see cref="IGridBehaviour"/> components or subclasses.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlaceableObject : MonoBehaviour, IPlaceable, IPowerHost
    {
        [SerializeField] private Vector2Int _size = Vector2Int.one;
        [SerializeField] private Vector2 _pivot01 = Vector2.zero;
        [SerializeField] private bool _allowOverlap;
        [SerializeField] private ObjectType _objectType = ObjectType.None;
        [SerializeField] private PlaceableItemKind _itemKind = PlaceableItemKind.Building;
        [SerializeField] private bool _isEnemyBuilding;
        [SerializeField] private bool _allowContinuousDrag;
        [SerializeField] private bool _continuousSpacingBySize;
        [SerializeField] private bool _forcePlaceAnywhere;
        [SerializeField] private bool _enableLineAssist;
        [SerializeField] private bool _canBeDestroyedByWave;
        [SerializeField] private int _buildingHealth = 10;
        [SerializeField] private string _saveId = "unique_object_id";
        [SerializeField] private int _sortingOrderWhenPlaced = -1;

        [SerializeField, HideInInspector] private List<Vector2Int> _occupiedCells = new List<Vector2Int>();

        private Vector2Int _bottomLeft;
        private bool _isPlaced;
        private IGridBehaviour[] _behaviours;
        private bool _behavioursResolved;
        private bool _powerComponentsCached;

        /// <summary>Raised when this object is consumed by crafting or inventory.</summary>
        public event Action Consumed;

        /// <summary>Raised on destruction so the grid and power services can unregister it.</summary>
        public event Action<IPlaceable> Destroyed;

        /// <inheritdoc />
        public Vector2Int Size => _size.x >= 1 && _size.y >= 1 ? _size : Vector2Int.one;

        /// <inheritdoc />
        public Vector2 Pivot01 => _pivot01;

        /// <inheritdoc />
        public bool AllowOverlap => _allowOverlap;

        /// <inheritdoc />
        public IReadOnlyList<Vector2Int> OccupiedCells => _occupiedCells;

        /// <inheritdoc />
        public Vector2Int BottomLeft => _bottomLeft;

        /// <inheritdoc />
        public bool IsPlaced => _isPlaced;

        /// <inheritdoc />
        public ObjectType ObjectType => _objectType;

        /// <inheritdoc />
        public PlaceableItemKind ItemKind => _itemKind;

        /// <inheritdoc />
        public bool IsEnemyBuilding => _isEnemyBuilding;

        /// <inheritdoc />
        public bool AllowContinuousDrag => _allowContinuousDrag;

        /// <inheritdoc />
        public bool ContinuousSpacingBySize => _continuousSpacingBySize;

        /// <inheritdoc />
        public bool ForcePlaceAnywhere => _forcePlaceAnywhere;

        /// <inheritdoc />
        public bool EnableLineAssist => _enableLineAssist;

        /// <inheritdoc />
        public bool CanBeDestroyedByWave => _canBeDestroyedByWave;

        /// <inheritdoc />
        public int BuildingHealth => _buildingHealth;

        /// <inheritdoc />
        public string SaveId => _saveId;

        /// <summary>Sorting order applied on placement. -1 keeps the prefab value.</summary>
        public int SortingOrderWhenPlaced => _sortingOrderWhenPlaced;

        /// <summary>Cached generator device, if present.</summary>
        public IPowerGenerator Generator { get; private set; }

        /// <summary>Cached consumer device, if present.</summary>
        public IPowerConsumer Consumer { get; private set; }

        /// <summary>Cached storage device, if present.</summary>
        public IPowerStorage Storage { get; private set; }

        /// <inheritdoc />
        public void SetPlacement(Vector2Int bottomLeft, IReadOnlyList<Vector2Int> cells)
        {
            _bottomLeft = bottomLeft;
            _occupiedCells.Clear();
            for (int i = 0; i < cells.Count; i++) _occupiedCells.Add(cells[i]);
            _isPlaced = true;
            OnPlaced();
        }

        /// <inheritdoc />
        public void ClearPlacement()
        {
            if (!_isPlaced) return;
            _occupiedCells.Clear();
            _isPlaced = false;
            OnRemoved();
        }

        /// <summary>Applies one fixed amount of damage and raises the shake hook.</summary>
        public void TakeHit()
        {
            if (_isEnemyBuilding || !_canBeDestroyedByWave || _buildingHealth <= 0) return;
            _buildingHealth--;
            OnDamaged();
            if (_buildingHealth <= 0) Destroy(gameObject);
        }

        /// <summary>Notifies attached behaviours after a successful commit.</summary>
        public void NotifyPlaced(GridPlacementService service)
        {
            EnsureBehaviours();
            for (int i = 0; i < _behaviours.Length; i++)
            {
                if (_behaviours[i] != null) _behaviours[i].OnPlaced(this, service);
            }
        }

        /// <summary>Notifies attached behaviours after removal.</summary>
        public void NotifyRemoved()
        {
            EnsureBehaviours();
            for (int i = 0; i < _behaviours.Length; i++)
            {
                if (_behaviours[i] != null) _behaviours[i].OnRemoved();
            }
        }

        /// <summary>Finds and caches power devices using a caller-owned component buffer.</summary>
        public void CachePowerComponents(List<Component> buffer)
        {
            if (_powerComponentsCached) return;
            _powerComponentsCached = true;

            buffer.Clear();
            GetComponents(buffer);

            for (int i = 0; i < buffer.Count; i++)
            {
                Component component = buffer[i];
                if (Generator == null && component is IPowerGenerator generator) Generator = generator;
                else if (Consumer == null && component is IPowerConsumer consumer) Consumer = consumer;
                else if (Storage == null && component is IPowerStorage storage) Storage = storage;
            }
        }

        /// <summary>Applies a complete archetype configuration.</summary>
        public void Configure(
            Vector2Int size,
            Vector2 pivot01,
            bool allowOverlap,
            ObjectType objectType,
            PlaceableItemKind itemKind,
            string saveId,
            int sortingOrderWhenPlaced,
            bool allowContinuousDrag,
            bool continuousSpacingBySize,
            bool forcePlaceAnywhere,
            bool enableLineAssist)
        {
            _size = new Vector2Int(Mathf.Max(1, size.x), Mathf.Max(1, size.y));
            _pivot01 = pivot01;
            _allowOverlap = allowOverlap;
            _objectType = objectType;
            _itemKind = itemKind;
            _saveId = string.IsNullOrEmpty(saveId) ? "unique_object_id" : saveId;
            _sortingOrderWhenPlaced = sortingOrderWhenPlaced;
            _allowContinuousDrag = allowContinuousDrag;
            _continuousSpacingBySize = continuousSpacingBySize;
            _forcePlaceAnywhere = forcePlaceAnywhere;
            _enableLineAssist = enableLineAssist;
        }

        /// <summary>Raises the consumed event.</summary>
        public void MarkConsumed()
        {
            Consumed?.Invoke();
        }

        /// <summary>Inheritance hook invoked by <see cref="SetPlacement"/>.</summary>
        protected virtual void OnPlaced() { }

        /// <summary>Inheritance hook invoked by <see cref="ClearPlacement"/>.</summary>
        protected virtual void OnRemoved() { }

        /// <summary>Inheritance hook invoked when building health changes.</summary>
        protected virtual void OnDamaged() { }

        private void EnsureBehaviours()
        {
            if (_behavioursResolved) return;
            _behaviours = GetComponents<IGridBehaviour>();
            _behavioursResolved = true;
        }

        private void OnDestroy()
        {
            Destroyed?.Invoke(this);
        }

        private void OnValidate()
        {
            _size.x = Mathf.Max(1, _size.x);
            _size.y = Mathf.Max(1, _size.y);
            _buildingHealth = Mathf.Max(1, _buildingHealth);
        }
    }
}
