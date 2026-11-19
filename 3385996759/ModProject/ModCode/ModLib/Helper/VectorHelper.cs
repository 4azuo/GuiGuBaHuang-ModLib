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

        /// <summary>Names of the 8 directions, counterclockwise from the right (screen: y up).</summary>
        public static readonly string[] DIRECTIONS8 = { "right", "up-right", "up", "up-left", "left", "down-left", "down", "down-right" };

        /// <summary>
        /// Direction of a move as one of 8 names (<see cref="DIRECTIONS8"/>), or "static" when it is shorter than
        /// <paramref name="minLength"/>.
        /// </summary>
        public static string ToDirection8(this Vector2 move, float minLength = 0.0001f)
        {
            if (move.magnitude < minLength) return "static";
            var a = (float)(Math.Atan2(move.y, move.x) * 180.0 / Math.PI);
            return DIRECTIONS8[(int)Mathf.Repeat(Mathf.Round(a / 45f), 8)];
        }
    }
}