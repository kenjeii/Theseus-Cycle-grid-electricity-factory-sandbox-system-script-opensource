using System.Collections.Generic;
using UnityEngine;

namespace TheseusCore.Grid.Samples
{
    /// <summary>
    /// Pure C# walkthrough: place a 2x1 object, then attempt an overlapping placement that
    /// is resolved by edge snap. No scene, prefab or MonoBehaviour required.
    /// </summary>
    public static class ProgrammaticPlacementExample
    {
        private sealed class ExamplePlaceable : IPlaceable
        {
            private readonly List<Vector2Int> _cells = new List<Vector2Int>();

            public Vector2Int Size { get; set; }
            public Vector2 Pivot01 => Vector2.zero;
            public bool AllowOverlap => false;
            public IReadOnlyList<Vector2Int> OccupiedCells => _cells;
            public Vector2Int BottomLeft { get; private set; }
            public bool IsPlaced { get; private set; }

            public void SetPlacement(Vector2Int bottomLeft, IReadOnlyList<Vector2Int> cells)
            {
                BottomLeft = bottomLeft;
                _cells.Clear();
                for (int i = 0; i < cells.Count; i++) _cells.Add(cells[i]);
                IsPlaced = true;
            }

            public void ClearPlacement()
            {
                _cells.Clear();
                IsPlaced = false;
            }
        }

        /// <summary>Runs the example and returns the second placement result.</summary>
        public static PlacementResult Run()
        {
            GridCoordinateSystem coordinates = new GridCoordinateSystem(Vector2.zero, 1f, new RectInt(0, 0, 16, 16));
            GridPlacementService service = new GridPlacementService(coordinates);
            service.AddRule(new BoundsRule());
            service.AddRule(new NoOverlapRule());

            ExamplePlaceable first = new ExamplePlaceable { Size = new Vector2Int(2, 1) };
            PlacementResult firstResult = service.Resolve(
                new Vector2Int(2, 2), first.Size, first.Pivot01, first.AllowOverlap, true, true, 4);
            service.Commit(first, firstResult);

            ExamplePlaceable second = new ExamplePlaceable { Size = new Vector2Int(2, 1) };
            PlacementResult secondResult = service.Resolve(
                new Vector2Int(3, 2), second.Size, second.Pivot01, second.AllowOverlap, true, true, 4);
            service.Commit(second, secondResult);

            return secondResult;
        }
    }
}
