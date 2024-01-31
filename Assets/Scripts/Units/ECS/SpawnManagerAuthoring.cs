using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SpawnManagerAuthoring : MonoBehaviour
{
    // The SpawnManager component will be used as a singleton.
    // It stores a grab bag of game parameters plus the entity prefabs that we'll instantiate at runtime.
    public Species PlayerOneSpecies;
    public Species PlayerTwoSpecies;
    public int NumberOfBaseSpawnerForPlayerOne;
    public int NumberOfBaseSpawnerForPlayerTwo;
    public float3 StartPositionBaseSpawnerPlayerOne;
    public float3 StartPositionBaseSpawnerPlayerTwo;
    public bool SpawnUnitWhenPressEnter;

    public int BasicSlimeUnitPerBaseSpawner;
    public int BasicMecaUnitPerBaseSpawner;

    public GameObject SlimeBaseSpawnerBuildingPrefab;
    public GameObject MecaBaseSpawnerBuildingPrefab;
    public GameObject BasicSlimeUnitPrefab;
    public GameObject BasicMecaUnitPrefab;

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
                NumberOfBaseSpawnerForPlayerOne = authoring.NumberOfBaseSpawnerForPlayerOne,
                NumberOfBaseSpawnerForPlayerTwo = authoring.NumberOfBaseSpawnerForPlayerTwo,
                StartPositionBaseSpawnerPlayerOne = authoring.StartPositionBaseSpawnerPlayerOne,
                StartPositionBaseSpawnerPlayerTwo = authoring.StartPositionBaseSpawnerPlayerTwo,
                SpawnUnitWhenPressEnter = authoring.SpawnUnitWhenPressEnter,
                BasicSlimeUnitPerBaseSpawner = authoring.BasicSlimeUnitPerBaseSpawner,
                BasicMecaUnitPerBaseSpawner = authoring.BasicMecaUnitPerBaseSpawner,
                // BasicSlimeUnitSpeed = authoring.BasicSlimeUnitSpeed,
                // BasicMecaUnitSpeed = authoring.BasicMecaUnitSpeed,
                // BasicSlimeUnitAttack = authoring.BasicSlimeUnitAttack,
                // BasicMecaUnitAttack = authoring.BasicMecaUnitAttack,

                // GetEntity() bakes a GameObject prefab into its entity equivalent.
                SlimeBaseSpawnerBuildingPrefab =
                    GetEntity(authoring.SlimeBaseSpawnerBuildingPrefab, TransformUsageFlags.Dynamic),
                MecaBaseSpawnerBuildingPrefab =
                    GetEntity(authoring.MecaBaseSpawnerBuildingPrefab, TransformUsageFlags.Dynamic),
                BasicSlimeUnitPrefab = GetEntity(authoring.BasicSlimeUnitPrefab, TransformUsageFlags.Dynamic),
                BasicMecaUnitPrefab = GetEntity(authoring.BasicMecaUnitPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct SpawnManager : IComponentData
{
    public Species PlayerOneSpecies;
    public Species PlayerTwoSpecies;
    public int NumberOfBaseSpawnerForPlayerOne;
    public int NumberOfBaseSpawnerForPlayerTwo;
    public float3 StartPositionBaseSpawnerPlayerOne;
    public float3 StartPositionBaseSpawnerPlayerTwo;
    public bool SpawnUnitWhenPressEnter;

    public int BasicSlimeUnitPerBaseSpawner;
    public int BasicMecaUnitPerBaseSpawner;

    public float BasicSlimeUnitSpeed; // meters per second
    public float BasicMecaUnitSpeed;
    public float BasicSlimeUnitAttack; // Amount of damage dealt by the slime
    public float BasicMecaUnitAttack;
    // BasicSlimeUnitLife, BasicMecaUnitLife ...

    public Entity SlimeBaseSpawnerBuildingPrefab;
    public Entity MecaBaseSpawnerBuildingPrefab;
    public Entity BasicSlimeUnitPrefab;
    public Entity BasicMecaUnitPrefab;
}