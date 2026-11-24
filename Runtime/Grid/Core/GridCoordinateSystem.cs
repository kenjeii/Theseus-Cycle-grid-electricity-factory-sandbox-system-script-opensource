using System.Collections.Generic;
using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>
    /// Immutable world-to-cell mapping. All conversions are allocation-free value operations.
    /// </summary>
    [System.Serializable]
    public struct GridCoordinateSystem
    {
        [SerializeField] private Vector2 _origin;
        [SerializeField] private float _cellSize;
        [SerializeField] private RectInt _buildBounds;

        /// <summary>Creates a coordinate system. Non-positive cell sizes fall back to one.</summary>
        public GridCoordinateSystem(Vector2 origin, float cellSize, RectInt buildBounds)
        {
            _origin = origin;
            _cellSize = cellSize > 0f ? cellSize : 1f;
            _buildBounds = buildBounds;
        }

        /// <summary>World position of grid cell (0,0).</summary>
        public Vector2 Origin => _origin;

        /// <summary>World units per cell. Always greater than zero.</summary>
        public float CellSize => _cellSize > 0f ? _cellSize : 1f;

        /// <summary>Inclusive cell range in which placement is allowed.</summary>
        public RectInt BuildBounds => _buildBounds;

        /// <summary>Converts a world point to the cell containing it.</summary>
        public Vector2Int WorldToGrid(Vector2 world)
        {
            Vector2 local = world - _origin;
            float inv = 1f / CellSize;
            return new Vector2Int(Mathf.FloorToInt(local.x * inv), Mathf.FloorToInt(local.y * inv));
        }

        /// <summary>
        /// Converts a footprint bottom-left and normalised pivot to a world position on z=0.
        /// </summary>
        public Vector3 GridToWorld(Vector2Int bottomLeft, Vector2Int size, Vector2 pivot01)
        {
            Vector2 pivot = new Vector2(Mathf.Clamp01(pivot01.x), Mathf.Clamp01(pivot01.y));
            float x = _origin.x + (bottomLeft.x + pivot.x * size.x) * CellSize;
            float y = _origin.y + (bottomLeft.y + pivot.y * size.y) * CellSize;
            return new Vector3(x, y, 0f);
        }

        /// <summary>
        /// Returns the bottom-left cell for a world point given a footprint size and pivot.
        /// </summary>
        public Vector2Int BottomLeftFromWorld(Vector2 world, Vector2Int size, Vector2 pivot01)
        {
            Vector2 pivot = new Vector2(Mathf.Clamp01(pivot01.x), Mathf.Clamp01(pivot01.y));
            Vector2 g = (world - _origin) / CellSize;
            float bx = Mathf.Floor(g.x - pivot.x * size.x + 1e-6f);
            float by = Mathf.Floor(g.y - pivot.y * size.y + 1e-6f);
            return new Vector2Int((int)bx, (int)by);
        }

        /// <summary>Fills a caller-owned buffer with every cell in a footprint. No allocation.</summary>
        public void FillCoveredCells(Vector2Int bottomLeft, Vector2Int size, List<Vector2Int> buffer)
        {
            buffer.Clear();
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    buffer.Add(new Vector2Int(bottomLeft.x + x, bottomLeft.y + y));
                }
            }
        }

        /// <summary>Clamps a bottom-left cell so the full footprint stays inside the bounds.</summary>
        /// <returns>True when the value changed.</returns>
        public bool ClampBottomLeft(ref Vector2Int bottomLeft, Vector2Int size)
        {
            Vector2Int clamped = bottomLeft;
            clamped.x = Mathf.Clamp(clamped.x, _buildBounds.xMin, _buildBounds.xMax - size.x);
            clamped.y = Mathf.Clamp(clamped.y, _buildBounds.yMin, _buildBounds.yMax - size.y);

            if (clamped == bottomLeft) return false;
            bottomLeft = clamped;
            return true;
        }

        /// <summary>Returns true when every cell lies inside the build bounds.</summary>
        public bool ContainsAll(IReadOnlyList<Vector2Int> cells)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                if (!_buildBounds.Contains(cells[i])) return false;
            }
            return true;
        }
    }
}
