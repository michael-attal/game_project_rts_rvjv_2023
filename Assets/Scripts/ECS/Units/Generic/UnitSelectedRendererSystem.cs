using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using EndSimulationEntityCommandBufferSystem = Unity.Entities.EndSimulationEntityCommandBufferSystem;
using EntityCommandBuffer = Unity.Entities.EntityCommandBuffer;
using ISystem = Unity.Entities.ISystem;
using SystemAPI = Unity.Entities.SystemAPI;
using SystemState = Unity.Entities.SystemState;

[UpdateAfter(typeof(UnitSelectableSystem))]
public partial struct UnitSelectedRendererSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<SpawnManager>();
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<Game>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();
        var gameManager = SystemAPI.GetSingleton<Game>();

        if (!configManager.ActivateUnitSelectedRendererSystem)
        {
            state.Enabled = false;
            return;
        }

        if (gameManager.State == GameState.Paused)
            return;

        var spawnManager = SystemAPI.GetSingleton<SpawnManager>();

        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        foreach (var (unitSelectable, entity) in SystemAPI.Query<RefRO<SelectionCircle>>().WithEntityAccess())
        {
            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();

        var ecb2 = new EntityCommandBuffer(Allocator.TempJob);
        foreach (var unitSelectedTransform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<UnitSelected>())
        {
            var entity = ecb2.Instantiate(spawnManager.SelectionCirclePrefab);
            ecb2.SetComponent(entity, new LocalTransform
            {
                Rotation = Quaternion.Euler(90, 0, 0),
                Scale = unitSelectedTransform.ValueRO.Scale * 100, // NOTE: The circle is 1cm square,
                Position = new float3(unitSelectedTransform.ValueRO.Position.x, 0, unitSelectedTransform.ValueRO.Position.z)
            });
            ecb2.SetComponent(entity, new SelectionCircle());
        }

        ecb2.Playback(state.EntityManager);
        ecb2.Dispose();
    }
}