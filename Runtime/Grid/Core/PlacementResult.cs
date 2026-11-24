using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>Describes how a candidate footprint was resolved.</summary>
    public enum PlacementResolution
    {
        /// <summary>No valid cell was found.</summary>
        Failed,

        /// <summary>The desired cell was accepted unchanged.</summary>
        Exact,

        /// <summary>The candidate was pushed away from occupied neighbours.</summary>
        EdgeSnapped,

        /// <summary>The candidate was moved to the nearest free cell.</summary>
        NearestFree
    }

    /// <summary>Value-type result of a placement resolution. No heap allocation.</summary>
    public struct PlacementResult
    {
        /// <summary>Whether a valid cell was found.</summary>
        public bool Success;

        /// <summary>Resolution strategy that produced the result.</summary>
        public PlacementResolution Resolution;

        /// <summary>Resolved bottom-left cell.</summary>
        public Vector2Int BottomLeft;

        /// <summary>Resolved world position at the caller's pivot.</summary>
        public Vector3 WorldPosition;

        /// <summary>Creates a failed result.</summary>
        public static PlacementResult Failure()
        {
            return new PlacementResult
            {
                Success = false,
                Resolution = PlacementResolution.Failed,
                BottomLeft = Vector2Int.zero,
                WorldPosition = Vector3.zero
            };
        }
    }
}
