using ModLib.Attributes;
using System;
using UnityEngine;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for vector calculations.
    /// Provides utilities for distance and range calculations with Vector2Int.
    /// </summary>
    [ActionCat("Vector")]
    public static class VectorHelper
    {
        /// <summary>
        /// Calculates minimum range between two grid positions.
        /// </summary>
        /// <param name="src">Source position</param>
        /// <param name="dest">Destination position</param>
        /// <returns>Minimum range</returns>
        public static int CalRange(this Vector2Int src, Vector2Int dest)
        {
            var newVec = src - dest;
            return Math.Min(Math.Abs(newVec.x), Math.Abs(newVec.y));
        }
    }
}