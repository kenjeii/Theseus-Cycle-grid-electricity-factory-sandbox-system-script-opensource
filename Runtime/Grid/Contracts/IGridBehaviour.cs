namespace TheseusCore.Grid
{
    /// <summary>
    /// Pluggable behaviour attached to a <see cref="IPlaceable"/>.
    /// This is the composition counterpart to subclassing PlaceableObject.
    /// </summary>
    public interface IGridBehaviour
    {
        /// <summary>Called after the owner has been committed to the occupancy map.</summary>
        void OnPlaced(IPlaceable placeable, GridPlacementService service);

        /// <summary>Called after the owner has been removed from the occupancy map.</summary>
        void OnRemoved();
    }
}
