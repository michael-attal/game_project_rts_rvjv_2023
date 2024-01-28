using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using ISystem = Unity.Entities.ISystem;
using Random = Unity.Mathematics.Random;
using SystemAPI = Unity.Entities.SystemAPI;
using SystemState = Unity.Entities.SystemState;


// This UpdateBefore is necessary to ensure the BasicSlime get rendered in
// the correct position for the frame in which they're spawned.
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct BasicSlimeSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BasicSlimeSpawner>();
        state.RequireForUpdate<SpawnManager>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var spawnManager = SystemAPI.GetSingleton<SpawnManager>();

        if (!Input.GetKeyDown(KeyCode.Return))
            return; // For the moment I spawn basic slime by pressing enter, later i will do it with a for loop and spawnManager.BasicSlimeUnitPerSpawner
        Debug.Log("Enter detected! Spawning BasicSlime Unit");

        var rand = new Random(123);

        // Spawn a BasicSlime, position it at the base spawner player's location, and give it a random velocity.
        foreach (var transform in
                 SystemAPI.Query<RefRO<LocalTransform>>()
                     .WithAll<Player>())
        {
            var basicSlime = state.EntityManager.Instantiate(spawnManager.BasicSlimePrefab);
            state.EntityManager.SetComponentData(basicSlime, new LocalTransform
            {
                Position = transform.ValueRO.Position,
                Rotation = quaternion.identity,
                Scale = 1
            });

            // TODO: Implement the handled system and the basic slime moving system. Currently.
            // I have implemented an auto target feature that selects the first encountered meca enemy.
            state.EntityManager.SetComponentData(basicSlime, new Velocity
            {
                // NextFloat2Direction() returns a random 2d unit vector.
                Value = rand.NextFloat2Direction()
                // In order to boost the initial velocity, we can multiply it by: Value * spawnManager.BasicSlimeStartVelocity. However, to accomplish this, we must include BasicSlimeStartVelocity as a public variable in the spawn manager.
            });
        }
    }
}