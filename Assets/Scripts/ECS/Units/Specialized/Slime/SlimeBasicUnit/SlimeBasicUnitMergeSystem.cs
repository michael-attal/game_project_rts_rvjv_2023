using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct SlimeMergeSystem : ISystem
{
    private EntityQuery query;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<SpawnManager>();
        state.RequireForUpdate<UnitSelected>();
        state.RequireForUpdate<SlimeBasicUnitMerge>();
        query = state.GetEntityQuery(typeof(SlimeBasicUnitMerge), typeof(UnitSelected), typeof(LocalToWorld));
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

        if (!Input.GetKeyDown(KeyCode.F)) return;

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        var spawnManager = SystemAPI.GetSingleton<SpawnManager>();
        var slimeStrongerUnitPrefab = spawnManager.SlimeStrongerUnitPrefab;

        var entities = query.ToEntityArray(Allocator.TempJob);
        var positions = query.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);

        // TODO: Get the minimum number of unit to be able to merge from spawn manager
        if (entities.Length < 10)
        {
            entities.Dispose();
            positions.Dispose();
            return;
        }

        var groupCount = entities.Length / 10;
        var defaultScale = state.EntityManager.GetComponentData<LocalTransform>(slimeStrongerUnitPrefab).Scale;

        var mergeUnitsJob = new MergeUnitsJob
        {
            ECB = ecb,
            MergedUnitPrefab = slimeStrongerUnitPrefab,
            DefaultScale = defaultScale,
            Entities = entities,
            Positions = positions,
            GroupCount = groupCount
        }.Schedule(groupCount, 1, state.Dependency);

        state.Dependency = mergeUnitsJob;
    }
}

[BurstCompile]
public struct MergeUnitsJob : IJobParallelFor
{
    public EntityCommandBuffer.ParallelWriter ECB;
    public Entity MergedUnitPrefab;
    public float DefaultScale;
    [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> Entities;
    [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<LocalToWorld> Positions;
    public int GroupCount;

    public void Execute(int index)
    {
        var averagePosition = float3.zero;
        for (var i = 0; i < 10; i++)
        {
            averagePosition += Positions[index * 10 + i].Position;
        }

        averagePosition /= 10;

        var newEntity = ECB.Instantiate(index, MergedUnitPrefab);

        ECB.SetComponent(index, newEntity, new LocalTransform
            {
                Position = averagePosition,
                Rotation = quaternion.identity,
                Scale = DefaultScale
            }
        );
        ECB.SetComponentEnabled<UnitSelected>(GroupCount, newEntity, true); // NOTE: Automatically select the new unit. I'm not sure if we should actually do that for the gameplay.

        for (var i = 0; i < 10; i++)
        {
            ECB.DestroyEntity(index, Entities[index * 10 + i]);
        }
    }
}