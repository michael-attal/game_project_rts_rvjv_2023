using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

// This UpdateBefore is necessary to ensure the unit get rendered in
// the correct position for the frame in which they're spawned.
// If the unit spawning system differs significantly between units, we should implement a specialized system, such as MySlimeUnitSpawningSystem, instead of a generic one like this one.
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct UnitSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<SpawnManager>();
        state.RequireForUpdate<BaseSpawnerBuilding>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();

        if (!configManager.ActivateUnitSpawnerSystem)
        {
            state.Enabled = false;
            return;
        }

        var spawnManager = SystemAPI.GetSingleton<SpawnManager>();

        if (spawnManager.SpawnUnitWhenPressEnter)
        {
            if (!Input.GetKeyDown(KeyCode.Return))
                return;
            Debug.Log("Enter detected! Spawning unit now!");
        }
        else
        {
            state.Enabled = false; // Let it spawn only one time if it does it automatically
        }

        // TODO: Create a job with IJobParallelFor to efficiently instantiate multiple units
        foreach (var (transform, baseSpawnerInfos) in
                 SystemAPI.Query<RefRO<LocalTransform>, RefRO<BaseSpawnerBuilding>>()
                     .WithAll<BaseSpawnerBuilding>())
            // Spawn a unit, position it at near the base spawner player's location, and give it a random velocity.
        {
            var nbOfUnitPerBaseSpawner = baseSpawnerInfos.ValueRO.NbOfUnitPerBase;

            var rows = 50 % nbOfUnitPerBaseSpawner;
            var cols = nbOfUnitPerBaseSpawner / rows;
            var spacer = 2; // Space between 10x10 units group
            var unitSpace = 1.2f; // space between units

            for (var x = 0; x < rows; x++)
            for (var z = 0; z < cols; z++)
            {
                var basicUnit = state.EntityManager.Instantiate(baseSpawnerInfos.ValueRO.SpawnedUnitPrefab);

                var extraXSpace = x % 10 == 0 ? spacer : 0;
                var extraZSpace = z % 10 == 0 ? spacer : 0;

                state.EntityManager.SetComponentData(basicUnit, new LocalTransform
                {
                    Position = new float3
                    {
                        x = x * unitSpace + extraXSpace - rows / 2 - spacer * 2,
                        y = transform.ValueRO.Position.y,
                        z = z * unitSpace + extraZSpace + transform.ValueRO.Position.z
                    },
                    Rotation = quaternion.identity,
                    Scale = 1
                });
            }
        }
    }
}