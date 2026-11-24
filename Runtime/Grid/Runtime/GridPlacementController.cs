using System.Collections.Generic;
using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>
    /// Unity-facing orchestrator: reads input, resolves a candidate through the pure C# service,
    /// drives the ghost preview and commits new placeables. All external concerns are injected.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GridPlacementController : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private GridCoordinateSystem _coordinates =
            new GridCoordinateSystem(Vector2.zero, 1f, new RectInt(0, 0, 1000, 1000));

        [SerializeField] private PlaceableFactory _factory;
        [SerializeField] private List<string> _placeableIds = new List<string>();

        [Header("Rules")]
        [SerializeField] private bool _clampInsideBounds = true;
        [SerializeField] private bool _useEdgeSnap = true;
        [SerializeField] private int _fallbackSearchRadius = 4;

        [Header("Preview")]
        [SerializeField] private Color _validColor = new Color(1f, 1f, 1f, 0.6f);
        [SerializeField] private Color _invalidColor = new Color(1f, 0.3f, 0.3f, 0.6f);
        [SerializeField] private int _ghostSortingOrder = 32767;

        private GridPlacementService _service;
        private PowerNetworkService _power;
        private GhostPresenter _ghost;
        private IPlacementInput _input;
        private ICursorWorldResolver _cursor;

        private int _currentIndex;
        private PlaceableObject _currentPrototype;
        private Vector2Int _desiredBottomLeft;
        private bool _hasCandidate;

        private void Awake()
        {
            if (_input == null) _input = GetComponent<IPlacementInput>();
            if (_cursor == null) _cursor = GetComponent<ICursorWorldResolver>();
            if (_factory == null) _factory = GetComponent<PlaceableFactory>();

            Initialize(_input, _cursor, _factory);
        }

        /// <summary>Injects external concerns and builds the placement service.</summary>
        public void Initialize(IPlacementInput input, ICursorWorldResolver cursor, PlaceableFactory factory)
        {
            _input = input;
            _cursor = cursor;
            if (factory != null) _factory = factory;

            _service = new GridPlacementService(_coordinates);
            _service.AddRule(new BoundsRule());
            _service.AddRule(new NoOverlapRule());
            _power = new PowerNetworkService(_service);

            _ghost = new GhostPresenter(transform, _ghostSortingOrder);
            ResolvePrototype();
        }

        /// <summary>The service driving this controller.</summary>
        public GridPlacementService Service => _service;

        /// <summary>The power topology service driven by placements.</summary>
        public PowerNetworkService Power => _power;

        /// <summary>True when the next placement is horizontally flipped.</summary>
        public bool FlipHorizontal { get; private set; }

        /// <summary>True when straight-line placement assist is active.</summary>
        public bool LineAssistEnabled { get; private set; }

        /// <summary>Toggles horizontal flip for subsequent placements.</summary>
        public void ToggleFlip()
        {
            FlipHorizontal = !FlipHorizontal;
        }

        /// <summary>Toggles straight-line placement assist.</summary>
        public void ToggleLineAssist()
        {
            LineAssistEnabled = !LineAssistEnabled;
        }

        private void LateUpdate()
        {
            if (_power != null) _power.RebuildDirtyNetworks();
        }

        /// <summary>Removes an instance from the grid and its power network.</summary>
        public void Remove(PlaceableObject instance)
        {
            if (instance == null) return;
            instance.Destroyed -= OnInstanceDestroyed;
            _power?.RemoveNode(instance);
            _service?.Remove(instance);
            instance.NotifyRemoved();
        }

        private void OnInstanceDestroyed(IPlaceable placeable)
        {
            if (placeable == null) return;
            _power?.RemoveNode(placeable);
            _service?.Remove(placeable);
        }

        private string CurrentId => _placeableIds.Count > 0 ? _placeableIds[_currentIndex] : string.Empty;

        private void Update()
        {
            if (_service == null || _input == null || _cursor == null || _factory == null) return;
            if (_placeableIds.Count == 0) return;

            if (_input.CancelPressed)
            {
                _ghost.Hide();
                _hasCandidate = false;
                return;
            }

            if (_input.CycleNextPressed)
            {
                _currentIndex = (_currentIndex + 1) % _placeableIds.Count;
                ResolvePrototype();
            }

            if (!_cursor.TryGetWorldPosition(out Vector3 world)) return;
            if (_currentPrototype == null) return;

            Vector2Int size = _currentPrototype.Size;
            Vector2 pivot = _currentPrototype.Pivot01;

            _desiredBottomLeft = _coordinates.BottomLeftFromWorld(world, size, pivot);
            if (_clampInsideBounds) _coordinates.ClampBottomLeft(ref _desiredBottomLeft, size);

            bool valid = _service.IsValidCandidate(_desiredBottomLeft, size, _currentPrototype.AllowOverlap);
            _ghost.Tint(valid ? _validColor : _invalidColor);
            _ghost.MoveTo(_coordinates.GridToWorld(_desiredBottomLeft, size, pivot));
            _hasCandidate = true;

            if (_input.PlacePressed) TryPlace();
        }

        private void TryPlace()
        {
            if (!_hasCandidate || _currentPrototype == null) return;

            PlacementResult result = _service.Resolve(
                _desiredBottomLeft,
                _currentPrototype.Size,
                _currentPrototype.Pivot01,
                _currentPrototype.AllowOverlap,
                _clampInsideBounds,
                _useEdgeSnap,
                _fallbackSearchRadius);

            if (!result.Success) return;

            PlaceableObject instance;
            if (!_factory.TryCreate(CurrentId, result.WorldPosition, Quaternion.identity, transform, out instance)) return;
            if (!_service.Commit(instance, result))
            {
                _factory.Release(instance);
                return;
            }

            instance.NotifyPlaced(_service);
            instance.Destroyed += OnInstanceDestroyed;
            _power.AddNode(instance);
            ShowGhost();
        }

        private void ResolvePrototype()
        {
            _currentPrototype = null;
            if (_placeableIds.Count == 0) return;
            if (_currentIndex >= _placeableIds.Count) _currentIndex = 0;

            GameObject prefab;
            if (!_factory.TryGetPrefab(CurrentId, out prefab) || prefab == null) return;
            _currentPrototype = prefab.GetComponent<PlaceableObject>();
            ShowGhost();
        }

        private void ShowGhost()
        {
            if (_ghost == null || _factory == null) return;
            if (_placeableIds.Count == 0) return;

            GameObject prefab;
            if (!_factory.TryGetPrefab(CurrentId, out prefab)) return;
            _ghost.Show(prefab);
        }
    }
}
