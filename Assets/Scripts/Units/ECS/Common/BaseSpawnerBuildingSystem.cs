using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct BaseSpawnerBuildingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnManager>();
        state.RequireForUpdate<BaseSpawnerBuilding>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // We only want to spawn base spawners players one time. Disabling the system stops subsequent updates.
        state.Enabled = false;

        var spawnManager = SystemAPI.GetSingleton<SpawnManager>();

        // For simplicity and consistency, we'll use a fixed random seed value.
        var randSlime = new Random(123);
        var randMeca = new Random(456);
        var scale = 1;

        // FIXME: Later do only one loop for for player one and player two
        // Instantiate player one slime base unit spawners (the bases that will produce the units, like BasicSlime).
        var offsetPlayerOne = spawnManager.NumberOfBaseSpawnerForPlayerOne == 1
            ? 0
            : randSlime.NextFloat(spawnManager.NumberOfBaseSpawnerForPlayerOne);
        for (var i = 0; i < spawnManager.NumberOfBaseSpawnerForPlayerOne; i++)
        {
            // Instantiate copies an entity: a new entity is created with all the same component types
            // and component values as the SlimeBaseSpawnerBuildingPrefab or MecaBaseSpawnerBuildingPrefab entity.
            var baseSpawnerPlayer =
                state.EntityManager.Instantiate(spawnManager.PlayerOneSpecies == Species.Slime
                    ? spawnManager.SlimeBaseSpawnerBuildingPrefab
                    : spawnManager.MecaBaseSpawnerBuildingPrefab);


            // Position the new base building spawner by setting its LocalTransform component.
            state.EntityManager.SetComponentData(baseSpawnerPlayer, new LocalTransform
            {
                Position = new float3
                {
                    x = spawnManager.StartPositionBaseSpawnerPlayerOne.x + offsetPlayerOne,
                    y = 1,
                    z = spawnManager.StartPositionBaseSpawnerPlayerOne.y + offsetPlayerOne
                },
                Scale = scale,
                Rotation = quaternion.identity
            });

            state.EntityManager.SetComponentData(baseSpawnerPlayer, new BaseSpawnerBuilding
            {
                species = spawnManager.PlayerOneSpecies
            });
        }

        // Instantiate player two meca base unit spawners (the bases that will produce the units, like BasicMeca).
        var offsetPlayerTwo = spawnManager.NumberOfBaseSpawnerForPlayerTwo == 1
            ? 0
            : randSlime.NextFloat(spawnManager.NumberOfBaseSpawnerForPlayerTwo);
        for (var i = 0; i < spawnManager.NumberOfBaseSpawnerForPlayerTwo; i++)
        {
            var baseSpawnerPlayer =
                state.EntityManager.Instantiate(spawnManager.PlayerTwoSpecies == Species.Slime
                    ? spawnManager.SlimeBaseSpawnerBuildingPrefab
                    : spawnManager.MecaBaseSpawnerBuildingPrefab);

            state.EntityManager.SetComponentData(baseSpawnerPlayer, new LocalTransform
            {
                Position = new float3
                {
                    x = spawnManager.StartPositionBaseSpawnerPlayerTwo.x + offsetPlayerTwo,
                    y = 1,
                    z = spawnManager.StartPositionBaseSpawnerPlayerTwo.y + offsetPlayerTwo
                },
                Scale = scale,
                Rotation = quaternion.identity
            });

            state.EntityManager.SetComponentData(baseSpawnerPlayer, new BaseSpawnerBuilding
            {
                species = spawnManager.PlayerTwoSpecies
            });
        }

        Debug.Log("Players base unit spawners building successfully created!");
    }
}