using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    public uint PlayerNumber;
    public SpeciesType PlayerSpecies;
    public uint NbOfBaseSpawnerBuilding = 1;
    public uint NbOfUnitPerBaseSpawnerBuilding = 1000;
    public float3 StartPosition;

    public GameObject BaseSpawnerBuildingPrefab;
    public GameObject BasicUnitPrefab;

    private class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new Player
            {
                PlayerNumber = authoring.PlayerNumber,
                PlayerSpecies = authoring.PlayerSpecies,
                NbOfBaseSpawnerBuilding = authoring.NbOfBaseSpawnerBuilding,
                NbOfUnitPerBaseSpawnerBuilding = authoring.NbOfUnitPerBaseSpawnerBuilding,
                StartPosition = authoring.StartPosition,
                Winner = false,

                BaseSpawnerBuildingPrefab = GetEntity(authoring.BaseSpawnerBuildingPrefab, TransformUsageFlags.Dynamic),
                BasicUnitPrefab = GetEntity(authoring.BasicUnitPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct Player : IComponentData
{
    public uint PlayerNumber;
    public SpeciesType PlayerSpecies;
    public uint NbOfBaseSpawnerBuilding;
    public uint NbOfUnitPerBaseSpawnerBuilding;
    public float3 StartPosition;
    public bool Winner; // NOTE: Create a component or use a data ?

    // public Entity PlayerHandPrefab;
    public Entity BaseSpawnerBuildingPrefab;
    public Entity BasicUnitPrefab;
}