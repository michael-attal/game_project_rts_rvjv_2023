using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    private class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<Player>(entity);
        }
    }
}

public struct Player : IComponentData
{
    public uint PlayerNumber;
    public float3 StartPosition;
    public uint StartNbOfBaseSpawnerBuilding;
}