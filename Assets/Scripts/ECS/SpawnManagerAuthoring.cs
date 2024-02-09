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

    public bool SpawnUnitWhenPressEnter;
    public GroupUnitShape GroupUnitsBy;

    public uint NumberOfBaseSpawnerForPlayerOne;
    public uint NumberOfBaseSpawnerForPlayerTwo;
    public uint NumberOfSlimeUnitPerSlimeBaseSpawner;
    public uint NumberOfMecaUnitPerMecaBaseSpawner;

    public GameObject SlimePlayerHandPrefab;
    public GameObject SlimeBaseSpawnerBuildingPrefab;
    public GameObject SlimeBasicUnitPrefab;

    public GameObject MecaPlayerHandPrefab;
    public GameObject MecaBaseSpawnerBuildingPrefab;
    public GameObject MecaBasicUnitPrefab;

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

                SpawnUnitWhenPressEnter = authoring.SpawnUnitWhenPressEnter,
                GroupUnitsBy = authoring.GroupUnitsBy,

                NumberOfBaseSpawnerForPlayerOne = authoring.NumberOfBaseSpawnerForPlayerOne,
                NumberOfBaseSpawnerForPlayerTwo = authoring.NumberOfBaseSpawnerForPlayerTwo,
                NumberOfSlimeUnitPerSlimeBaseSpawner = authoring.NumberOfSlimeUnitPerSlimeBaseSpawner,
                NumberOfMecaUnitPerMecaBaseSpawner = authoring.NumberOfMecaUnitPerMecaBaseSpawner,

                // SlimeBasicUnitSpeed = authoring.SlimeBasicUnitSpeed,
                // SlimeBasicUnitAttack = authoring.SlimeBasicUnitAttack,
                // MecaBasicUnitSpeed = authoring.MecaBasicUnitSpeed,
                // MecaBasicUnitAttack = authoring.MecaBasicUnitAttack,

                // GetEntity() bakes a GameObject prefab into its entity equivalent.
                SlimePlayerHandPrefab =
                    GetEntity(authoring.SlimePlayerHandPrefab, TransformUsageFlags.Dynamic),
                SlimeBasicUnitPrefab = GetEntity(authoring.SlimeBasicUnitPrefab, TransformUsageFlags.Dynamic),
                SlimeBaseSpawnerBuildingPrefab =
                    GetEntity(authoring.SlimeBaseSpawnerBuildingPrefab, TransformUsageFlags.Dynamic),

                MecaPlayerHandPrefab =
                    GetEntity(authoring.MecaPlayerHandPrefab, TransformUsageFlags.Dynamic),
                MecaBaseSpawnerBuildingPrefab =
                    GetEntity(authoring.MecaBaseSpawnerBuildingPrefab, TransformUsageFlags.Dynamic),
                MecaBasicUnitPrefab = GetEntity(authoring.MecaBasicUnitPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct SpawnManager : IComponentData
{
    public SpeciesType PlayerOneSpecies;
    public SpeciesType PlayerTwoSpecies;
    public float3 StartPositionBaseSpawnerPlayerOne;
    public float3 StartPositionBaseSpawnerPlayerTwo;

    public bool SpawnUnitWhenPressEnter;
    public GroupUnitShape GroupUnitsBy;

    public uint NumberOfBaseSpawnerForPlayerOne;
    public uint NumberOfBaseSpawnerForPlayerTwo;
    public uint NumberOfSlimeUnitPerSlimeBaseSpawner;
    public uint NumberOfMecaUnitPerMecaBaseSpawner;

    public float SlimeBasicUnitSpeed; // meters per second
    public float SlimeBasicUnitAttack; // Amount of damage dealt by the slime
    public float MecaBasicUnitSpeed;
    public float MecaBasicUnitAttack;
    // SlimeBasicUnitLife, MecaBasicUnitLife ...

    public Entity SlimePlayerHandPrefab;
    public Entity SlimeBaseSpawnerBuildingPrefab;
    public Entity SlimeBasicUnitPrefab;

    public Entity MecaPlayerHandPrefab;
    public Entity MecaBaseSpawnerBuildingPrefab;
    public Entity MecaBasicUnitPrefab;
}