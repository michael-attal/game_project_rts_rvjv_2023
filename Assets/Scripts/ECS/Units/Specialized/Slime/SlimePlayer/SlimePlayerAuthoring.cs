using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SlimePlayerAuthoring : MonoBehaviour
{
    public uint PlayerNumber;
    public uint NbOfBaseSpawnerBuilding = 1;
    public uint NbOfUnitPerBaseSpawnerBuilding = 1000;
    public float3 StartPosition;

    private class Baker : Baker<SlimePlayerAuthoring>
    {
        public override void Bake(SlimePlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new SlimePlayer
            {
                PlayerNumber = authoring.PlayerNumber,
                NbOfBaseSpawnerBuilding = authoring.NbOfBaseSpawnerBuilding,
                NbOfUnitPerBaseSpawnerBuilding = authoring.NbOfUnitPerBaseSpawnerBuilding,
                StartPosition = authoring.StartPosition,
                Winner = false
            });
        }
    }
}

public struct SlimePlayer : IComponentData
{
    public uint PlayerNumber;
    public uint NbOfBaseSpawnerBuilding;
    public uint NbOfUnitPerBaseSpawnerBuilding;
    public float3 StartPosition;
    public bool Winner; // NOTE: Create a component or use a data ?
}