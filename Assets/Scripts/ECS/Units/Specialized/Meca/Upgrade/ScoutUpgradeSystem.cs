using Unity.Burst;
using Unity.Entities;
using UnityEngine;

partial struct ScoutUpgradeSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<UnitAttack>();
        state.RequireForUpdate<UnitDamage>();
        state.RequireForUpdate<ScoutUpgrade>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();

        if (!configManager.ActivateMecaBasicUnitUpgradeSystem)
        {
            state.Enabled = false;
            return;
        }

        if (configManager.IsGamePaused)
            return;
        
        var job = new ScoutUpgradeJob
        {
            ECB = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
        };
        job.ScheduleParallel();
    }
}

[BurstCompile]
[WithAll(typeof(ScoutUpgrade))]
public partial struct ScoutUpgradeJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ECB;

    private void Execute(Entity entity, RefRW<MovementManual> unitInfo, RefRW<UnitAttack> unitAttack, [ChunkIndexInQuery] int chunkIndex)
    {
        unitInfo.ValueRW.Speed *= 1.5f;
        unitAttack.ValueRW.Strength = 5;
        ECB.RemoveComponent<ScoutUpgrade>(chunkIndex, entity);
    }
}

public struct ScoutUpgrade : IComponentData {}