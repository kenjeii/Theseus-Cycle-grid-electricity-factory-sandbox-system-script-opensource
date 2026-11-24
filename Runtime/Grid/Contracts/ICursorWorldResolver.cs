using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>Converts a pointing device position into a world-space grid query point.</summary>
    public interface ICursorWorldResolver
    {
        /// <summary>Returns false when no valid cursor position exists this frame.</summary>
        bool TryGetWorldPosition(out Vector3 worldPosition);
    }
}
