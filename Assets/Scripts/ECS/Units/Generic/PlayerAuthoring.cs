using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    public uint PlayerNumber;
    public SpeciesType PlayerSpecies;
    public float3 StartPosition;

    public GameObject BaseSpawnerBuildingPrefab;
    public uint NbOfBaseSpawnerBuilding = 1;

    private class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new Player
            {
                PlayerNumber = authoring.PlayerNumber,
                PlayerSpecies = authoring.PlayerSpecies,
                StartPosition = authoring.StartPosition,

                BaseSpawnerBuildingPrefab = GetEntity(authoring.BaseSpawnerBuildingPrefab, TransformUsageFlags.Dynamic),
                NbOfBaseSpawnerBuilding = authoring.NbOfBaseSpawnerBuilding
            });
        }
    }
}

public struct Player : IComponentData
{
    public uint PlayerNumber;
    public SpeciesType PlayerSpecies;
    public float3 StartPosition;

    // public Entity PlayerHandPrefab;
    public Entity BaseSpawnerBuildingPrefab;
    public uint NbOfBaseSpawnerBuilding;
}