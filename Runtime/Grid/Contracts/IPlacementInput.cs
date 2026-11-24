namespace TheseusCore.Grid
{
    /// <summary>
    /// Input adapter for the placement controller. Keeps the core free of any
    /// dependency on Unity's legacy Input or Input System packages.
    /// </summary>
    public interface IPlacementInput
    {
        /// <summary>Advance to the next placeable prefab.</summary>
        bool CycleNextPressed { get; }

        /// <summary>Commit the current candidate.</summary>
        bool PlacePressed { get; }

        /// <summary>Abort the current placement mode.</summary>
        bool CancelPressed { get; }
    }
}
