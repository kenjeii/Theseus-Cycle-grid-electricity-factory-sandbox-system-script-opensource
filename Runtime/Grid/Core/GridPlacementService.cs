using System.Collections.Generic;
using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>
    /// Pure C# placement pipeline: coordinate conversion, validity rules, edge snap resolution,
    /// nearest-free fallback and occupancy commit. Scratch buffers are reused so repeated calls
    /// from Update produce no managed allocations.
    /// </summary>
    public sealed class GridPlacementService
    {
        private readonly GridCoordinateSystem _coordinates;
        private readonly GridOccupancyMap _occupancy = new GridOccupancyMap();
        private readonly List<IPlacementRule> _rules = new List<IPlacementRule>();

        private readonly List<Vector2Int> _scratchCells = new List<Vector2Int>();
        private readonly List<Vector2Int> _overlapped = new List<Vector2Int>();
        private readonly List<Vector2Int> _candidateCells = new List<Vector2Int>();

        /// <summary>Creates a service over a coordinate system.</summary>
        public GridPlacementService(GridCoordinateSystem coordinates)
        {
            _coordinates = coordinates;
        }

        /// <summary>The coordinate system in use.</summary>
        public GridCoordinateSystem Coordinates => _coordinates;

        /// <summary>The live occupancy index.</summary>
        public GridOccupancyMap Occupancy => _occupancy;

        /// <summary>Appends a validity rule if it is not already registered.</summary>
        public void AddRule(IPlacementRule rule)
        {
            if (rule != null && !_rules.Contains(rule)) _rules.Add(rule);
        }

        /// <summary>Removes all validity rules.</summary>
        public void ClearRules()
        {
            _rules.Clear();
        }

        /// <summary>Evaluates all rules against a candidate footprint.</summary>
        public bool CanPlaceAt(IReadOnlyList<Vector2Int> cells, bool allowOverlap)
        {
            if (!allowOverlap && _occupancy.ContainsAny(cells)) return false;

            for (int i = 0; i < _rules.Count; i++)
            {
                if (!_rules[i].CanPlace(cells, _occupancy, _coordinates.BuildBounds)) return false;
            }
            return true;
        }

        /// <summary>Convenience check for a bottom-left cell and footprint.</summary>
        public bool IsValidCandidate(Vector2Int bottomLeft, Vector2Int size, bool allowOverlap)
        {
            _coordinates.FillCoveredCells(bottomLeft, size, _scratchCells);
            return CanPlaceAt(_scratchCells, allowOverlap);
        }

        /// <summary>
        /// Resolves a desired footprint into a valid cell using edge snap then nearest-free search.
        /// </summary>
        public PlacementResult Resolve(
            Vector2Int desiredBottomLeft,
            Vector2Int size,
            Vector2 pivot01,
            bool allowOverlap,
            bool clampInsideBounds,
            bool useEdgeSnap,
            int fallbackSearchRadius)
        {
            Vector2Int bottomLeft = desiredBottomLeft;
            if (clampInsideBounds) _coordinates.ClampBottomLeft(ref bottomLeft, size);

            _coordinates.FillCoveredCells(bottomLeft, size, _scratchCells);
            bool forbidden = !CanPlaceAt(_scratchCells, allowOverlap);
            PlacementResolution resolution = PlacementResolution.Exact;

            if (forbidden)
            {
                bool resolved = false;

                if (useEdgeSnap && TryEdgeSnap(bottomLeft, size, allowOverlap, clampInsideBounds, out Vector2Int snapped))
                {
                    bottomLeft = snapped;
                    resolution = PlacementResolution.EdgeSnapped;
                    resolved = true;
                }

                if (!resolved && fallbackSearchRadius > 0 &&
                    TryFindNearestFree(bottomLeft, size, allowOverlap, clampInsideBounds, fallbackSearchRadius, out Vector2Int nearest))
                {
                    bottomLeft = nearest;
                    resolution = PlacementResolution.NearestFree;
                    resolved = true;
                }

                if (!resolved) return PlacementResult.Failure();
            }

            _coordinates.FillCoveredCells(bottomLeft, size, _scratchCells);
            if (!CanPlaceAt(_scratchCells, allowOverlap)) return PlacementResult.Failure();

            return new PlacementResult
            {
                Success = true,
                Resolution = resolution,
                BottomLeft = bottomLeft,
                WorldPosition = _coordinates.GridToWorld(bottomLeft, size, pivot01)
            };
        }

        /// <summary>
        /// Registers a placeable at a resolved position. Re-validates before committing.
        /// </summary>
        public bool Commit(IPlaceable placeable, PlacementResult result)
        {
            if (placeable == null || !result.Success) return false;

            _coordinates.FillCoveredCells(result.BottomLeft, placeable.Size, _scratchCells);
            if (!CanPlaceAt(_scratchCells, placeable.AllowOverlap)) return false;

            placeable.SetPlacement(result.BottomLeft, _scratchCells);
            _occupancy.Add(placeable);
            return true;
        }

        /// <summary>Unregisters a placeable and clears its placement state.</summary>
        public void Remove(IPlaceable placeable)
        {
            if (placeable == null) return;
            _occupancy.Remove(placeable);
            placeable.ClearPlacement();
        }

        /// <summary>
        /// Collects distinct objects sharing any of the target's cells or their four neighbours.
        /// Used by the power network to discover adjacent devices in O(cells * occupants).
        /// </summary>
        public void CollectNeighbours(IPlaceable placeable, List<IPlaceable> output, HashSet<IPlaceable> seen)
        {
            output.Clear();
            if (placeable == null) return;

            HashSet<IPlaceable> visited = seen ?? _fallbackSeen;
            visited.Clear();
            visited.Add(placeable);

            IReadOnlyList<Vector2Int> cells = placeable.OccupiedCells;
            for (int i = 0; i < cells.Count; i++)
            {
                Vector2Int cell = cells[i];
                CollectCellOccupants(cell, output, visited);
                for (int d = 0; d < 4; d++) CollectCellOccupants(cell + NeighbourDirections[d], output, visited);
            }
        }

        private void CollectCellOccupants(Vector2Int cell, List<IPlaceable> output, HashSet<IPlaceable> visited)
        {
            List<IPlaceable> occupants;
            if (!_occupancy.TryGet(cell, out occupants)) return;

            for (int i = 0; i < occupants.Count; i++)
            {
                IPlaceable candidate = occupants[i];
                if (candidate == null || candidate.ObjectType == ObjectType.None) continue;
                if (visited.Add(candidate)) output.Add(candidate);
            }
        }

        private static readonly Vector2Int[] NeighbourDirections =
        {
            Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
        };

        private readonly HashSet<IPlaceable> _fallbackSeen = new HashSet<IPlaceable>();

        /// <summary>
        /// Pushes the footprint past the outermost occupied neighbours in the cheapest direction.
        /// Mirrors the original four-direction edge snap with zero LINQ allocations.
        /// </summary>
        private bool TryEdgeSnap(
            Vector2Int bottomLeft,
            Vector2Int size,
            bool allowOverlap,
            bool clampInsideBounds,
            out Vector2Int resolved)
        {
            resolved = bottomLeft;
            _occupancy.CollectOccupiedCells(_scratchCells, _overlapped);
            if (_overlapped.Count == 0) return true;

            int maxRight = int.MinValue;
            int minLeft = int.MaxValue;
            int maxUp = int.MinValue;
            int minDown = int.MaxValue;

            for (int i = 0; i < _overlapped.Count; i++)
            {
                Vector2Int cell = _overlapped[i];
                if (cell.x + 1 > maxRight) maxRight = cell.x + 1;
                if (cell.x - size.x < minLeft) minLeft = cell.x - size.x;
                if (cell.y + 1 > maxUp) maxUp = cell.y + 1;
                if (cell.y - size.y < minDown) minDown = cell.y - size.y;
            }

            int pushRight = maxRight - bottomLeft.x;
            int pushLeft = bottomLeft.x - minLeft;
            int pushUp = maxUp - bottomLeft.y;
            int pushDown = bottomLeft.y - minDown;

            int bestCost = int.MaxValue;
            bool found = false;

            for (int direction = 0; direction < 4; direction++)
            {
                Vector2Int candidate;
                int cost;

                switch (direction)
                {
                    case 0:
                        candidate = new Vector2Int(bottomLeft.x + pushRight, bottomLeft.y);
                        cost = pushRight;
                        break;
                    case 1:
                        candidate = new Vector2Int(bottomLeft.x - pushLeft, bottomLeft.y);
                        cost = pushLeft;
                        break;
                    case 2:
                        candidate = new Vector2Int(bottomLeft.x, bottomLeft.y + pushUp);
                        cost = pushUp;
                        break;
                    default:
                        candidate = new Vector2Int(bottomLeft.x, bottomLeft.y - pushDown);
                        cost = pushDown;
                        break;
                }

                if (cost <= 0 || cost >= bestCost) continue;

                _coordinates.FillCoveredCells(candidate, size, _candidateCells);
                if (clampInsideBounds && !_coordinates.ContainsAll(_candidateCells)) continue;
                if (!CanPlaceAt(_candidateCells, allowOverlap)) continue;

                resolved = candidate;
                bestCost = cost;
                found = true;
            }

            return found;
        }

        /// <summary>
        /// Ring search by increasing Chebyshev radius, returning the candidate with the smallest
        /// squared distance inside the first ring that contains a valid cell.
        /// </summary>
        private bool TryFindNearestFree(
            Vector2Int center,
            Vector2Int size,
            bool allowOverlap,
            bool clampInsideBounds,
            int radius,
            out Vector2Int resolved)
        {
            resolved = center;
            int bestDistanceSquared = int.MaxValue;
            bool found = false;

            for (int r = 0; r <= radius; r++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    for (int dy = -r; dy <= r; dy++)
                    {
                        if (r > 0 && Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) != r) continue;

                        Vector2Int candidate = new Vector2Int(center.x + dx, center.y + dy);
                        _coordinates.FillCoveredCells(candidate, size, _candidateCells);
                        if (clampInsideBounds && !_coordinates.ContainsAll(_candidateCells)) continue;
                        if (!CanPlaceAt(_candidateCells, allowOverlap)) continue;

                        int distanceSquared = dx * dx + dy * dy;
                        if (distanceSquared < bestDistanceSquared)
                        {
                            bestDistanceSquared = distanceSquared;
                            resolved = candidate;
                            found = true;
                        }
                    }
                }

                if (found) return true;
            }

            return false;
        }
    }
}
