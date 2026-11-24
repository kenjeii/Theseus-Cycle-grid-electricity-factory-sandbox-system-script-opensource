using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>8x15 decoy placeable that permits overlap.</summary>
    [DisallowMultipleComponent]
    public sealed class FakeManPlaceable : PlaceableObject
    {
        private void Reset()
        {
            PlaceableCatalog.FakeMan.ApplyTo(this);
        }
    }
}
