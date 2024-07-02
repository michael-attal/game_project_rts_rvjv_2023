using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SpawnManagerAuthoring : MonoBehaviour
{
    // The SpawnManager component will be used as a singleton.
    // It stores a grab bag of game parameters plus the entity prefabs that we'll instantiate at runtime.
    public SpeciesType PlayerOneSpecies;
    public SpeciesType PlayerTwoSpecies;

    public float3 StartPositionBaseSpawnerPlayerOne;
    public float3 StartPositionBaseSpawnerPlayerTwo;

    public uint NumberOfBaseSpawnerForPlayerOne;
    public uint NumberOfBaseSpawnerForPlayerTwo;

    public GameObject SelectionCirclePrefab;

    public GameObject SlimePlayerHandPrefab;
    public GameObject MecaPlayerHandPrefab;

    public GameObject SlimeBaseSpawnerBuildingPrefab;
    public GameObject MecaBaseSpawnerBuildingPrefab;

    private class Baker : Baker<SpawnManagerAuthoring>
    {
        public override void Bake(SpawnManagerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            // Each authoring field corresponds to a component field of the same name.
            AddComponent(entity, new SpawnManager
            {
                PlayerOneSpecies = authoring.PlayerOneSpecies,
                PlayerTwoSpecies = authoring.PlayerTwoSpecies,

                StartPositionBaseSpawnerPlayerOne = authoring.StartPositionBaseSpawnerPlayerOne,
                StartPositionBaseSpawnerPlayerTwo = authoring.StartPositionBaseSpawnerPlayerTwo,


                NumberOfBaseSpawnerForPlayerOne = authoring.NumberOfBaseSpawnerForPlayerOne,
                NumberOfBaseSpawnerForPlayerTwo = authoring.NumberOfBaseSpawnerForPlayerTwo,

                SelectionCirclePrefab = GetEntity(authoring.SelectionCirclePrefab, TransformUsageFlags.Dynamic),

                SlimePlayerHandPrefab = GetEntity(authoring.SlimePlayerHandPrefab, TransformUsageFlags.Dynamic),
                MecaPlayerHandPrefab = GetEntity(authoring.MecaPlayerHandPrefab, TransformUsageFlags.Dynamic),

                SlimeBaseSpawnerBuildingPrefab = GetEntity(authoring.SlimeBaseSpawnerBuildingPrefab, TransformUsageFlags.Dynamic),
                MecaBaseSpawnerBuildingPrefab = GetEntity(authoring.MecaBaseSpawnerBuildingPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct SpawnManager : IComponentData
{
    public SpeciesType PlayerOneSpecies;
    public SpeciesType PlayerTwoSpecies;

    public Entity SelectionCirclePrefab;

    public Entity SlimePlayerHandPrefab;
    public Entity MecaPlayerHandPrefab;

    public Entity SlimeBaseSpawnerBuildingPrefab;
    public Entity MecaBaseSpawnerBuildingPrefab;

    public uint NumberOfBaseSpawnerForPlayerOne;
    public uint NumberOfBaseSpawnerForPlayerTwo;

    public float3 StartPositionBaseSpawnerPlayerOne;
    public float3 StartPositionBaseSpawnerPlayerTwo;
}