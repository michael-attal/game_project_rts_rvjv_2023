using Unity.Mathematics;
using UnityEngine;

public static class Float3Extensions
{
    public static float DistanceTo(this float3 pos1, float3 pos2)
    {
        float sqrX = Mathf.Pow((pos2.x - pos1.x), 2);
        float sqrY = Mathf.Pow((pos2.y - pos1.y), 2);
        float sqrZ = Mathf.Pow((pos2.z - pos1.z), 2);
        return Mathf.Sqrt(sqrX + sqrY + sqrZ);
    }
}
