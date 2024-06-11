using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

// NOTE: Consider implementing a generic merging system that utilizes a MergeUnit component to specify the unit to be instantiated and the number of units to be merged, in order to create a merged unit.
[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct SlimeBasicUnitMergeSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<Game>();
        state.RequireForUpdate<SpawnManager>();
        state.RequireForUpdate<UnitSelectable>();
        state.RequireForUpdate<SlimeBasicUnitMerge>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();

        if (!configManager.ActivateSlimeBasicUnitMergeSystem)
        {
            state.Enabled = false;
            return;
        }

        if (configManager.IsGamePaused)
            return;

        // NOTE: We have to press multiple time F to merge unit if we have like 50 unit selected.
        if (!Input.GetKeyDown(KeyCode.F))
            return;

        // TODO: Implement a component, IsSelected, which is dynamically added or removed when a unit is selected (similar to the IsMovingTag component). This will eliminate the need for a nested loop to determine the number of selected entities, as the where option cannot be used in a Unity ECS query.

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var sumPositions = float3.zero;
        var totalEntities = 0;

        var fusionInfo = new FusionInfo();
        foreach (var (transform, merge, entity)
                 in SystemAPI.Query<RefRO<LocalToWorld>, RefRO<SlimeBasicUnitMerge>>()
                     .WithAll<UnitSelected>()
                     .WithEntityAccess())
        {
            ecb.DestroyEntity(entity);

            fusionInfo += merge.ValueRO.FusionInfo;
            sumPositions += transform.ValueRO.Position;
            ++totalEntities;
        }

        var gameInfo = SystemAPI.GetSingleton<Game>();
        var particleManager = SystemAPI.GetSingleton<ParticleManager>();
        for (var i = 0; i < gameInfo.SlimeRecipes.Value.Data.Length; ++i)
        {
            while (gameInfo.SlimeRecipes.Value.Data[i].Cost <= fusionInfo)
            {
                fusionInfo -= gameInfo.SlimeRecipes.Value.Data[i].Cost;

                var newEntity = InstantiateEntity(ref state, ecb, gameInfo.SlimeRecipes.Value.Data[i].PrefabId);
                ecb.SetComponent(newEntity, new LocalTransform
                {
                    Position = sumPositions / totalEntities,
                    Rotation = quaternion.identity,
                    Scale = 1f
                });

                // TODO: Refactor this and instantiate multiple generator for better effect
                var particleGenerator = ecb.Instantiate(particleManager.ParticleGeneratorPrefab);
                ecb.SetComponent(particleGenerator, new ParticleGeneratorData
                {
                    Rate = 5f,
                    LifetimeOfGenerator = 1f,
                    LifetimeOfParticle = 1f,
                    Size = 1f,
                    Speed = 2f,
                    Direction = new float3(0, 1, 0),
                    Color = new float4(0, 0, 1, 1)
                });
                ecb.SetComponent(particleGenerator, new LocalTransform
                {
                    Position = sumPositions / totalEntities,
                    Rotation = quaternion.identity,
                    Scale = 1f
                });
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private Entity InstantiateEntity(ref SystemState state, EntityCommandBuffer ecb, int id)
    {
        var buffer = SystemAPI.GetBuffer<InstantiatableEntityData>(SystemAPI.GetSingletonEntity<Game>());
        for (var i = 0; i < buffer.Length; ++i)
        {
            if (buffer[i].EntityID == id)
                return ecb.Instantiate(buffer[i].Entity);
        }

        return Entity.Null;
    }
}