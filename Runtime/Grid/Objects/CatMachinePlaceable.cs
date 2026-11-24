using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>Animated 33x15 machine that permits overlap.</summary>
    [DisallowMultipleComponent]
    public sealed class CatMachinePlaceable : PlaceableObject
    {
        private void Reset()
        {
            PlaceableCatalog.CatMachine.ApplyTo(this);
        }
    }
}
