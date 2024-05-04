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
        state.RequireForUpdate<SpawnManager>();
        state.RequireForUpdate<UnitSelectable>();
        state.RequireForUpdate<SlimeBasicUnitMerge>();
    }

    //[BurstCompile] can't burst compile, spawnManager is a ref now
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

        var spawnManagerQuery = SystemAPI.QueryBuilder().WithAll<SpawnManager>().Build();
        var spawnManager = spawnManagerQuery.GetSingleton<SpawnManager>();

        // TODO: Implement a component, IsSelected, which is dynamically added or removed when a unit is selected (similar to the IsMovingTag component). This will eliminate the need for a nested loop to determine the number of selected entities, as the where option cannot be used in a Unity ECS query.

        var query = SystemAPI.QueryBuilder()
            .WithAll<SlimeBasicUnitMerge>()
            .WithAll<UnitSelected>()
            .Build();

        var nbOfBasicSlimeUnitToMergeSelected = 0;
        uint nbOfUnitToAllowMerge = 10;
        var sumPositions = float3.zero;

        foreach (var entity in query.ToEntityArray(Allocator.Temp))
        {
            var unitSelectable = state.EntityManager.GetComponentData<UnitSelectable>(entity);
            var slimeBasicUnitMerge = state.EntityManager.GetComponentData<SlimeBasicUnitMerge>(entity);

            nbOfUnitToAllowMerge = slimeBasicUnitMerge.NbUnitsToMerge;

            if (unitSelectable.IsSelected)
            {
                nbOfBasicSlimeUnitToMergeSelected++;
            }
        }

        var unitMerged = 0;
        var stopMergeAt = nbOfBasicSlimeUnitToMergeSelected - nbOfBasicSlimeUnitToMergeSelected % 10;

        if (nbOfBasicSlimeUnitToMergeSelected > nbOfUnitToAllowMerge)
        {
            // TODO: Create a job to enable burst compile for this action
            foreach (var entity in query.ToEntityArray(Allocator.Temp))
            {
                var unitSelectable = state.EntityManager.GetComponentData<UnitSelectable>(entity);
                if (unitSelectable.IsSelected && unitMerged < stopMergeAt)
                {
                    var ltw = state.EntityManager.GetComponentData<LocalToWorld>(entity);
                    sumPositions += ltw.Position;

                    state.EntityManager.DestroyEntity(entity);

                    if ((unitMerged + 1) % nbOfUnitToAllowMerge == 0)
                    {
                        var slimeStrongerUnit = state.EntityManager.Instantiate(spawnManager.SlimeStrongerUnitPrefab);
                        var slimeStrongerUnitScale = state.EntityManager.GetComponentData<LocalTransform>(slimeStrongerUnit).Scale;

                        var averagePosition = sumPositions / nbOfUnitToAllowMerge;

                        state.EntityManager.SetComponentData(slimeStrongerUnit, new LocalTransform
                            {
                                Position = averagePosition,
                                Rotation = quaternion.identity,
                                Scale = slimeStrongerUnitScale
                            }
                        );

                        sumPositions = float3.zero; // Put back to zero for the next merging
                    }

                    unitMerged++;
                }
            }

            Debug.Log("Merging selected basic slime unit now!");
        }
    }
}