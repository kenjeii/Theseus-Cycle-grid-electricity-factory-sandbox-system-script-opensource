using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>Horizontal 17x6 solid obstacle variant.</summary>
    [DisallowMultipleComponent]
    public sealed class HorizontalBlockPlaceable : PlaceableObject
    {
        private void Reset()
        {
            PlaceableCatalog.BlockHorizontal.ApplyTo(this);
        }
    }
}
