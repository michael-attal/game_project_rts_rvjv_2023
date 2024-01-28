using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct PlayerSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnManager>();
        state.RequireForUpdate<PlayerSpawner>();
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

        // Instantiate player one slime base unit spawners (the bases that will produce the units, like BasicSlime).
        for (var i = 0; i < spawnManager.NumberOfPlayerOneSpawner; i++)
        {
            // Instantiate copies an entity: a new entity is created with all the same component types
            // and component values as the SlimeBasePrefab entity.
            var slimeBaseSpawner = state.EntityManager.Instantiate(spawnManager.SlimeBaseSpawnerPrefab);

            // Position the new base spawner by setting its LocalTransform component.
            state.EntityManager.SetComponentData(slimeBaseSpawner, new LocalTransform
            {
                Position = new float3
                {
                    x = i + randSlime.NextFloat(spawnManager.PlayerOneSpawnerOffset),
                    y = 0,
                    z = i + randSlime.NextFloat(spawnManager.PlayerOneSpawnerOffset)
                },
                Scale = scale,
                Rotation = quaternion.identity
            });
        }

        // Instantiate player two meca base unit spawners (the bases that will produce the units, like BasicMeca).
        for (var i = 0; i < spawnManager.NumberOfPlayerTwoSpawner; i++)
        {
            var mecaBaseSpawner = state.EntityManager.Instantiate(spawnManager.MecaBaseSpawnerPrefab);

            state.EntityManager.SetComponentData(mecaBaseSpawner, new LocalTransform
            {
                Position = new float3
                {
                    x = i + randMeca.NextFloat(spawnManager.PlayerTwoSpawnerOffset),
                    y = 0,
                    z = i + randMeca.NextFloat(spawnManager.PlayerTwoSpawnerOffset)
                },
                Scale = scale,
                Rotation = quaternion.identity
            });
        }

        Debug.Log("Players base unit spawners successfully created!");
    }
}