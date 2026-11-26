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

        /// <summary>The vector turned by <paramref name="deg"/> degrees (counterclockwise: screen, y up).</summary>
        public static Vector2 Rotate(this Vector2 v, float deg)
        {
            var r = deg * (float)(Math.PI / 180.0);
            var c = Mathf.Cos(r);
            var s = Mathf.Sin(r);
            return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
        }

        /// <summary>Angle of the vector in degrees (-180..180, 0: right, 90: up).</summary>
        public static float AngleDeg(this Vector2 v) => Mathf.Atan2(v.y, v.x) * (float)(180.0 / Math.PI);
    }
}