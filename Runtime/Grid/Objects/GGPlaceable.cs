using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>28x10 centred prop placeable.</summary>
    [DisallowMultipleComponent]
    public sealed class GGPlaceable : PlaceableObject
    {
        private void Reset()
        {
            PlaceableCatalog.Gg.ApplyTo(this);
        }
    }
}
