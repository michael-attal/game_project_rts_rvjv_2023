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

        // NOTE: We have to press multiple time F to merge unit if we have like 50 unit selected.
        if (!Input.GetKeyDown(KeyCode.F))
            return;
        
        // TODO: Implement a component, IsSelected, which is dynamically added or removed when a unit is selected (similar to the IsMovingTag component). This will eliminate the need for a nested loop to determine the number of selected entities, as the where option cannot be used in a Unity ECS query.

        var ecb = new EntityCommandBuffer(Allocator.Temp);
        
        var sumPositions = float3.zero;
        int totalEntities = 0;
        
        var fusionInfo = new FusionInfo();
        foreach (var (transform, merge, selectable, entity) 
                 in SystemAPI.Query<RefRO<LocalToWorld>, RefRO<SlimeBasicUnitMerge>, RefRO<UnitSelectable>>()
                     .WithEntityAccess())
        {
            if (!selectable.ValueRO.IsSelected)
                continue;
            
            ecb.DestroyEntity(entity);

            fusionInfo += merge.ValueRO.FusionInfo;
            sumPositions += transform.ValueRO.Position;
            ++totalEntities;
        }


        var gameInfo = SystemAPI.GetSingleton<Game>();
        for (int i = 0; i < gameInfo.SlimeRecipes.Value.Data.Length; ++i)
        {
            while (gameInfo.SlimeRecipes.Value.Data[i].Cost <= fusionInfo)
            {
                fusionInfo -= gameInfo.SlimeRecipes.Value.Data[i].Cost;
                Debug.Log(gameInfo.SlimeRecipes.Value.Data[i].EntityPrefab);

                var newEntity = ecb.Instantiate(gameInfo.SlimeRecipes.Value.Data[i].EntityPrefab);
                ecb.SetComponent(newEntity, new LocalTransform()
                {
                    Position = float3.zero,
                    Rotation = quaternion.identity,
                    Scale = 10000
                });
            }
        }
        
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}