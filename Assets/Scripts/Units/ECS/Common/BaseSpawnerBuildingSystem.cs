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

        // TODO: [REFACTOR] Later, streamline the code by combining the loops for player one and player two. Add necessary information, such as the number of base spawners, to the Player : IComponentData. Then, use a system query to retrieve each player component and execute the code within a single loop.
        // Instantiate player one slime base unit spawners (the bases that will produce the units, like BasicSlimeUnit).
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
                    y = spawnManager.StartPositionBaseSpawnerPlayerOne.y,
                    z = spawnManager.StartPositionBaseSpawnerPlayerOne.z + offsetPlayerOne
                },
                Scale = scale,
                Rotation = quaternion.identity
            });

            state.EntityManager.SetComponentData(baseSpawnerPlayer, new BaseSpawnerBuilding
            {
                species = spawnManager.PlayerOneSpecies
            });
        }

        // Instantiate player two meca base unit spawners (the bases that will produce the units, like BasicMecaUnit).
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
                    y = spawnManager.StartPositionBaseSpawnerPlayerTwo.y,
                    z = spawnManager.StartPositionBaseSpawnerPlayerTwo.z + offsetPlayerTwo
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