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
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();

        if (!configManager.ActivateUnitSelectedRendererSystem)
        {
            state.Enabled = false;
            return;
        }

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

        // Destroy existing selection circles
        // var destroySelectionCircleJob = new DestroySelectionCircleJob
        // {
        //     ECB = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
        //         .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
        // };
        // destroySelectionCircleJob.ScheduleParallel();

        // Create an EntityQuery to count entities with UnitSelected component
        // var query = state.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<UnitSelected>(), ComponentType.ReadOnly<LocalTransform>());
        // var numberOfEntities = query.CalculateEntityCount();
        //
        // if (numberOfEntities > 0)
        // {
        //     var ecbJob = new EntityCommandBuffer(Allocator.TempJob);
        //
        //     var addSelectionCircleJob = new AddSelectionCircleJob
        //     {
        //         ECB = ecbJob.AsParallelWriter(),
        //         SelectionCirclePrefab = spawnManager.SelectionCirclePrefab,
        //         Positions = new NativeArray<float3>(numberOfEntities, Allocator.TempJob),
        //         Scales = new NativeArray<float>(numberOfEntities, Allocator.TempJob),
        //         Rotations = new NativeArray<quaternion>(numberOfEntities, Allocator.TempJob)
        //     };
        //
        //     var index = 0;
        //     foreach (var unitSelectedTransform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<UnitSelected>())
        //     {
        //         addSelectionCircleJob.Positions[index] = new float3(unitSelectedTransform.ValueRO.Position.x, 0, unitSelectedTransform.ValueRO.Position.z);
        //         addSelectionCircleJob.Scales[index] = unitSelectedTransform.ValueRO.Scale * 100; // NOTE: The circle is 1cm square,
        //         addSelectionCircleJob.Rotations[index] = Quaternion.Euler(90, 0, 0);
        //         index++;
        //     }
        //
        //     var addSelectionCircleJobHandler = addSelectionCircleJob.Schedule(numberOfEntities, 64, state.Dependency);
        //     state.Dependency = addSelectionCircleJobHandler;
        //
        //     addSelectionCircleJobHandler.Complete();
        //     addSelectionCircleJob.Positions.Dispose();
        //     addSelectionCircleJob.Scales.Dispose();
        //     addSelectionCircleJob.Rotations.Dispose();
        //
        //     ecbJob.Playback(state.EntityManager);
        //     ecbJob.Dispose();
        // }
    }
}

// [WithAll(typeof(SelectionCircle))]
// public partial struct DestroySelectionCircleJob : IJobEntity
// {
//     public EntityCommandBuffer.ParallelWriter ECB;
//
//     private void Execute(Entity entity, RefRO<SelectionCircle> selectionCircle, [ChunkIndexInQuery] int chunkIndex)
//     {
//         ECB.DestroyEntity(chunkIndex, entity);
//     }
// }
//
// public struct AddSelectionCircleJob : IJobParallelFor
// {
//     public EntityCommandBuffer.ParallelWriter ECB;
//     public Entity SelectionCirclePrefab;
//     public NativeArray<float3> Positions;
//     public NativeArray<quaternion> Rotations;
//     public NativeArray<float> Scales;
//
//     public void Execute(int index)
//     {
//         var entity = ECB.Instantiate(index, SelectionCirclePrefab);
//         ECB.SetComponent(index, entity, new LocalTransform
//         {
//             Position = Positions[index],
//             Scale = Scales[index],
//             Rotation = Rotations[index]
//         });
//
//         ECB.SetComponent(index, entity, new SelectionCircle());
//     }
// }