using System.Collections.Generic;
using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>
    /// A stateless validity rule evaluated against a candidate cell set.
    /// Rules must not allocate or mutate the occupancy map.
    /// </summary>
    public interface IPlacementRule
    {
        /// <summary>Returns true when the candidate footprint satisfies this rule.</summary>
        bool CanPlace(IReadOnlyList<Vector2Int> cells, GridOccupancyMap occupancy, RectInt bounds);
    }
}
