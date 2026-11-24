using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>
    /// 21x37 teleport structure. Pairs with <see cref="TeleportLinkBehaviour"/>.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TeleportLinkBehaviour))]
    public sealed class TeleportPlaceable : PlaceableObject
    {
        private void Reset()
        {
            PlaceableCatalog.Teleport.ApplyTo(this);
        }
    }
}
