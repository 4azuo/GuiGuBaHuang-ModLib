using ModLib.Attributes;
using System;
using UnityEngine;

namespace ModLib.Helper
{
    [ActionCat("Vector")]
    public static class VectorHelper
    {
        public static int CalRange(this Vector2Int src, Vector2Int dest)
        {
            var newVec = src - dest;
            return Math.Min(Math.Abs(newVec.x), Math.Abs(newVec.y));
        }
    }
}