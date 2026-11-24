using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>Solid 17x6 obstacle from the original prefab set.</summary>
    [DisallowMultipleComponent]
    public sealed class BlockPlaceable : PlaceableObject
    {
        private void Reset()
        {
            PlaceableCatalog.Block.ApplyTo(this);
        }
    }
}
