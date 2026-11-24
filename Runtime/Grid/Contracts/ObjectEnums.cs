namespace TheseusCore.Grid
{
    /// <summary>Role of a placeable in the power network.</summary>
    public enum ObjectType
    {
        None,
        Wire,
        Generator,
        Storage,
        Consumer
    }

    /// <summary>Inventory classification of a placeable.</summary>
    public enum PlaceableItemKind
    {
        Building,
        Consumable,
        Material
    }
}
