using System.Collections.Generic;
using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>
    /// Sparse cell index that supports several placeables sharing one cell (stacking and wires).
    /// Lookups are O(1); enumerations use caller-owned buffers.
    /// </summary>
    public sealed class GridOccupancyMap
    {
        private readonly Dictionary<Vector2Int, List<IPlaceable>> _cells =
            new Dictionary<Vector2Int, List<IPlaceable>>();

        /// <summary>Number of distinct occupied cells.</summary>
        public int CellCount => _cells.Count;

        /// <summary>Returns the occupants of a cell, if any.</summary>
        public bool TryGet(Vector2Int cell, out List<IPlaceable> occupants)
        {
            return _cells.TryGetValue(cell, out occupants);
        }

        /// <summary>Returns true when at least one occupant exists in the given cells.</summary>
        public bool ContainsAny(IReadOnlyList<Vector2Int> cells)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                List<IPlaceable> occupants;
                if (_cells.TryGetValue(cells[i], out occupants) && occupants.Count > 0) return true;
            }
            return false;
        }

        /// <summary>Collects distinct occupants of the given cells into caller-owned buffers.</summary>
        public void CollectOccupants(IReadOnlyList<Vector2Int> cells, List<IPlaceable> output, HashSet<IPlaceable> seen)
        {
            output.Clear();
            if (seen != null) seen.Clear();

            for (int i = 0; i < cells.Count; i++)
            {
                List<IPlaceable> occupants;
                if (!_cells.TryGetValue(cells[i], out occupants)) continue;

                for (int j = 0; j < occupants.Count; j++)
                {
                    IPlaceable candidate = occupants[j];
                    if (candidate == null) continue;
                    if (seen != null && !seen.Add(candidate)) continue;
                    output.Add(candidate);
                }
            }
        }

        /// <summary>Collects the subset of cells that already have occupants.</summary>
        public void CollectOccupiedCells(IReadOnlyList<Vector2Int> cells, List<Vector2Int> output)
        {
            output.Clear();
            for (int i = 0; i < cells.Count; i++)
            {
                List<IPlaceable> occupants;
                if (_cells.TryGetValue(cells[i], out occupants) && occupants.Count > 0) output.Add(cells[i]);
            }
        }

        /// <summary>Indexes every cell of a footprint.</summary>
        public void Add(IPlaceable placeable)
        {
            if (placeable == null) return;

            IReadOnlyList<Vector2Int> cells = placeable.OccupiedCells;
            for (int i = 0; i < cells.Count; i++)
            {
                Vector2Int cell = cells[i];
                List<IPlaceable> occupants;
                if (!_cells.TryGetValue(cell, out occupants))
                {
                    occupants = new List<IPlaceable>();
                    _cells[cell] = occupants;
                }
                if (!occupants.Contains(placeable)) occupants.Add(placeable);
            }
        }

        /// <summary>Removes a placeable from every indexed cell it owns.</summary>
        public void Remove(IPlaceable placeable)
        {
            if (placeable == null) return;

            IReadOnlyList<Vector2Int> cells = placeable.OccupiedCells;
            for (int i = 0; i < cells.Count; i++)
            {
                Vector2Int cell = cells[i];
                List<IPlaceable> occupants;
                if (!_cells.TryGetValue(cell, out occupants)) continue;

                occupants.Remove(placeable);
                if (occupants.Count == 0) _cells.Remove(cell);
            }
        }

        /// <summary>Drops all entries.</summary>
        public void Clear()
        {
            _cells.Clear();
        }
    }
}
