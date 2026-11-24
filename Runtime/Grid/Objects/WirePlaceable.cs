using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>
    /// 2x4 wire tile. Pairs with <see cref="WireConnectableBehaviour"/> for neighbour-mask visuals.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(WireConnectableBehaviour))]
    public sealed class WirePlaceable : PlaceableObject
    {
        private void Reset()
        {
            PlaceableCatalog.Line2.ApplyTo(this);
        }
    }
}
