using System.Collections.Generic;
using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>
    /// Contract for an object that occupies a rectangular footprint of grid cells.
    /// Implementations must copy the cell list passed to <see cref="SetPlacement"/>.
    /// </summary>
    public interface IPlaceable
    {
        Vector2Int Size { get; }
        Vector2 Pivot01 { get; }
        bool AllowOverlap { get; }
        IReadOnlyList<Vector2Int> OccupiedCells { get; }
        Vector2Int BottomLeft { get; }
        bool IsPlaced { get; }

        ObjectType ObjectType { get; }
        PlaceableItemKind ItemKind { get; }
        bool IsEnemyBuilding { get; }
        bool AllowContinuousDrag { get; }
        bool ContinuousSpacingBySize { get; }
        bool ForcePlaceAnywhere { get; }
        bool EnableLineAssist { get; }
        bool CanBeDestroyedByWave { get; }
        int BuildingHealth { get; }
        string SaveId { get; }

        void SetPlacement(Vector2Int bottomLeft, IReadOnlyList<Vector2Int> cells);
        void ClearPlacement();
        void TakeHit();
    }
}
