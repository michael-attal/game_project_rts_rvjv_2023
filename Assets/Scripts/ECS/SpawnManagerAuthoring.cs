using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SpawnManagerAuthoring : MonoBehaviour
{
    // The SpawnManager component will be used as a singleton.
    // It stores a grab bag of game parameters plus the entity prefabs that we'll instantiate at runtime.
    [SerializeField] private SpeciesType playerOneSpecies;
    [SerializeField] private SpeciesType playerTwoSpecies;

    [SerializeField] private float3 startPositionBaseSpawnerSlime;
    [SerializeField] private float3 startPositionBaseSpawnerMeca;

    [SerializeField] private uint numberOfStartingBaseSpawnerForSlime;
    [SerializeField] private uint numberOfStartingBaseSpawnerForMeca;

    [SerializeField] private GameObject selectionCirclePrefab;

    [SerializeField] private GameObject slimePlayerHandPrefab;
    [SerializeField] private GameObject mecaPlayerHandPrefab;

    [SerializeField] private GameObject slimeBaseSpawnerBuildingPrefab;
    [SerializeField] private GameObject mecaBaseSpawnerBuildingPrefab;

    private class Baker : Baker<SpawnManagerAuthoring>
    {
        public override void Bake(SpawnManagerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            // Each authoring field corresponds to a component field of the same name.
            AddComponent(entity, new SpawnManager
            {
                PlayerOneSpecies = authoring.playerOneSpecies,
                PlayerTwoSpecies = authoring.playerTwoSpecies,

                StartPositionBaseSpawnerSlime = authoring.startPositionBaseSpawnerSlime,
                StartPositionBaseSpawnerMeca = authoring.startPositionBaseSpawnerMeca,

                NumberOfStartingBaseSpawnerForSlime = authoring.numberOfStartingBaseSpawnerForSlime,
                NumberOfStartingBaseSpawnerForMeca = authoring.numberOfStartingBaseSpawnerForMeca,

                SelectionCirclePrefab = GetEntity(authoring.selectionCirclePrefab, TransformUsageFlags.Dynamic),

                SlimePlayerHandPrefab = GetEntity(authoring.slimePlayerHandPrefab, TransformUsageFlags.Dynamic),
                MecaPlayerHandPrefab = GetEntity(authoring.mecaPlayerHandPrefab, TransformUsageFlags.Dynamic),

                SlimeBaseSpawnerBuildingPrefab = GetEntity(authoring.slimeBaseSpawnerBuildingPrefab, TransformUsageFlags.Dynamic),
                MecaBaseSpawnerBuildingPrefab = GetEntity(authoring.mecaBaseSpawnerBuildingPrefab, TransformUsageFlags.Dynamic)
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

    public uint NumberOfStartingBaseSpawnerForSlime;
    public uint NumberOfStartingBaseSpawnerForMeca;

    public float3 StartPositionBaseSpawnerSlime;
    public float3 StartPositionBaseSpawnerMeca;
}