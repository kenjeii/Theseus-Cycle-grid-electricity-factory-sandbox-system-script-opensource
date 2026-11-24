using System.Collections.Generic;
using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>Rejects footprints that leave the build bounds.</summary>
    public sealed class BoundsRule : IPlacementRule
    {
        /// <inheritdoc />
        public bool CanPlace(IReadOnlyList<Vector2Int> cells, GridOccupancyMap occupancy, RectInt bounds)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                if (!bounds.Contains(cells[i])) return false;
            }
            return true;
        }
    }

    /// <summary>Rejects any footprint that intersects an occupied cell.</summary>
    public sealed class NoOverlapRule : IPlacementRule
    {
        /// <inheritdoc />
        public bool CanPlace(IReadOnlyList<Vector2Int> cells, GridOccupancyMap occupancy, RectInt bounds)
        {
            return !occupancy.ContainsAny(cells);
        }
    }

    /// <summary>Composes rules with short-circuit evaluation in insertion order.</summary>
    public sealed class CompositeRule : IPlacementRule
    {
        private readonly List<IPlacementRule> _rules = new List<IPlacementRule>();

        /// <summary>Creates a composite from an optional set of rules.</summary>
        public CompositeRule(params IPlacementRule[] rules)
        {
            if (rules == null) return;
            for (int i = 0; i < rules.Length; i++) Add(rules[i]);
        }

        /// <summary>Appends a rule if not already present.</summary>
        public void Add(IPlacementRule rule)
        {
            if (rule != null && !_rules.Contains(rule)) _rules.Add(rule);
        }

        /// <inheritdoc />
        public bool CanPlace(IReadOnlyList<Vector2Int> cells, GridOccupancyMap occupancy, RectInt bounds)
        {
            for (int i = 0; i < _rules.Count; i++)
            {
                if (!_rules[i].CanPlace(cells, occupancy, bounds)) return false;
            }
            return true;
        }
    }
}
