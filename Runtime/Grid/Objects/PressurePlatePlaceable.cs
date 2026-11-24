using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>
    /// 17x6 pressure plate. Pairs with <see cref="PressureTriggerBehaviour"/>.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PressureTriggerBehaviour))]
    public sealed class PressurePlatePlaceable : PlaceableObject
    {
        private void Reset()
        {
            PlaceableCatalog.Step.ApplyTo(this);
        }
    }
}
