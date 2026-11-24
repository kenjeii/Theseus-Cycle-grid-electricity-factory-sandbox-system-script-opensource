using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>
    /// Immutable value-type archetype describing one placeable object. Mirrors the
    /// serialized fields of the original PlaceableObject component so a consumer can
    /// create a prefab or runtime instance without shipping any game assets.
    /// </summary>
    public struct PlaceableDefinition
    {
        /// <summary>Unique catalog key (prefab name in the source project).</summary>
        public string Id;

        /// <summary>Save identifier authored on the original prefab.</summary>
        public string SaveId;

        /// <summary>English display name from the source localization table.</summary>
        public string DisplayName;

        /// <summary>Footprint in cells.</summary>
        public Vector2Int Size;

        /// <summary>Pivot measured from the bottom-left cell. May be outside 0..1.</summary>
        public Vector2 Pivot01;

        /// <summary>Whether this object may share cells with another.</summary>
        public bool AllowOverlap;

        /// <summary>Role in the power network.</summary>
        public ObjectType PowerRole;

        /// <summary>Inventory classification.</summary>
        public PlaceableItemKind ItemKind;

        /// <summary>Sorting order applied on placement. -1 preserves the prefab value.</summary>
        public int SortingOrderWhenPlaced;

        /// <summary>Allow click-drag continuous placement.</summary>
        public bool AllowContinuousDrag;

        /// <summary>Step by footprint size while drag-placing.</summary>
        public bool ContinuousSpacingBySize;

        /// <summary>Ignore overlap restrictions during placement.</summary>
        public bool ForcePlaceAnywhere;

        /// <summary>Enable line-assist snapping while placing.</summary>
        public bool EnableLineAssist;

        /// <summary>Maximum stack size in inventory.</summary>
        public int MaxStack;

        /// <summary>
        /// Creates a definition with only the essential grid fields. Optional metadata
        /// defaults to neutral values so callers can build minimal definitions quickly.
        /// </summary>
        public PlaceableDefinition(
            string id,
            int width,
            int height,
            float pivotX,
            float pivotY,
            bool allowOverlap,
            int sortingOrderWhenPlaced)
        {
            Id = id;
            SaveId = id;
            DisplayName = id;
            Size = new Vector2Int(width, height);
            Pivot01 = new Vector2(pivotX, pivotY);
            AllowOverlap = allowOverlap;
            PowerRole = ObjectType.None;
            ItemKind = PlaceableItemKind.Building;
            SortingOrderWhenPlaced = sortingOrderWhenPlaced;
            AllowContinuousDrag = false;
            ContinuousSpacingBySize = false;
            ForcePlaceAnywhere = false;
            EnableLineAssist = false;
            MaxStack = 999;
        }

        /// <summary>Applies this definition to a placeable instance or prefab.</summary>
        public void ApplyTo(PlaceableObject target)
        {
            if (target == null) return;
            target.Configure(
                Size,
                Pivot01,
                AllowOverlap,
                PowerRole,
                ItemKind,
                SaveId,
                SortingOrderWhenPlaced,
                AllowContinuousDrag,
                ContinuousSpacingBySize,
                ForcePlaceAnywhere,
                EnableLineAssist);
        }
    }
}
