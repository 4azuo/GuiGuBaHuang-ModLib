using System;
using UnityEngine;

public static class VectorHelper
{
    public static int CalRange(this Vector2Int src, Vector2Int dest)
    {
        var newVec = src - dest;
        return Math.Min(Math.Abs(newVec.x), Math.Abs(newVec.y));
    }
}
