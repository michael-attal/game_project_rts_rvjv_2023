using Unity.Entities;
using UnityEngine;

public class SpawnManagerAuthoring : MonoBehaviour
{
    // The SpawnManager component will be used as a singleton.
    // It stores a grab bag of game parameters plus the entity prefabs that we'll instantiate at runtime.
    public int NumberOfPlayerOneSpawner;
    public int NumberOfPlayerTwoSpawner;
    public float PlayerOneSpawnerOffset;
    public float PlayerTwoSpawnerOffset;

    public float BasicSlimeUnitPerSpawner;
    public float BasicMecaUnitPerSpawner;

    public GameObject SlimeBaseSpawnerPrefab;
    public GameObject MecaBaseSpawnerPrefab;
    public GameObject BasicSlimePrefab;
    public GameObject BasicMecaPrefab;

    private class Baker : Baker<SpawnManagerAuthoring>
    {
        public override void Bake(SpawnManagerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            // Each authoring field corresponds to a component field of the same name.
            AddComponent(entity, new SpawnManager
            {
                NumberOfPlayerOneSpawner = authoring.NumberOfPlayerOneSpawner,
                NumberOfPlayerTwoSpawner = authoring.NumberOfPlayerTwoSpawner,
                PlayerOneSpawnerOffset = authoring.PlayerOneSpawnerOffset,
                PlayerTwoSpawnerOffset = authoring.PlayerTwoSpawnerOffset,
                BasicSlimeUnitPerSpawner = authoring.BasicSlimeUnitPerSpawner,
                BasicMecaUnitPerSpawner = authoring.BasicMecaUnitPerSpawner,
                // BasicSlimeSpeed = authoring.BasicSlimeSpeed,
                // BasicMecaSpeed = authoring.BasicMecaSpeed,
                // BasicSlimeAttack = authoring.BasicSlimeAttack,
                // BasicMecaAttack = authoring.BasicMecaAttack,

                // GetEntity() bakes a GameObject prefab into its entity equivalent.
                SlimeBaseSpawnerPrefab = GetEntity(authoring.SlimeBaseSpawnerPrefab, TransformUsageFlags.Dynamic),
                MecaBaseSpawnerPrefab = GetEntity(authoring.MecaBaseSpawnerPrefab, TransformUsageFlags.Dynamic),
                BasicSlimePrefab = GetEntity(authoring.BasicSlimePrefab, TransformUsageFlags.Dynamic),
                BasicMecaPrefab = GetEntity(authoring.BasicMecaPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct SpawnManager : IComponentData
{
    public int NumberOfPlayerOneSpawner;
    public int NumberOfPlayerTwoSpawner;
    public float PlayerOneSpawnerOffset;
    public float PlayerTwoSpawnerOffset;

    public float BasicSlimeUnitPerSpawner;
    public float BasicMecaUnitPerSpawner;

    public float BasicSlimeSpeed; // meters per second
    public float BasicMecaSpeed;
    public float BasicSlimeAttack; // Amount of damage dealt by the slime
    public float BasicMecaAttack;
    // BasicSlimeLife, BasicMecaLife ...

    public Entity SlimeBaseSpawnerPrefab;
    public Entity MecaBaseSpawnerPrefab;
    public Entity BasicSlimePrefab;
    public Entity BasicMecaPrefab;
}