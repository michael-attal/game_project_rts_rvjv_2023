using System;
using Unity.Mathematics;
using UnityEngine;

public static class Float3Extensions
{
    public static float DistanceTo(this float3 pos1, float3 pos2)
    {
        var sqrX = Mathf.Pow(pos2.x - pos1.x, 2);
        var sqrY = Mathf.Pow(pos2.y - pos1.y, 2);
        var sqrZ = Mathf.Pow(pos2.z - pos1.z, 2);
        return Mathf.Sqrt(sqrX + sqrY + sqrZ);
    }

    public static bool AreFloat3Equal(this float3 a, float3 b)
    {
        const float tolerance = 0.00001f;
        return Math.Abs(a.x - b.x) < tolerance && Math.Abs(a.y - b.y) < tolerance && Math.Abs(a.z - b.z) < tolerance;
    }
}