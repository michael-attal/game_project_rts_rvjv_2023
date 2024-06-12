using Unity.Burst;
using Unity.Entities;

partial struct GatlingUpgradeSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<UnitAttack>();
        state.RequireForUpdate<UnitDamage>();
        state.RequireForUpdate<GatlingUpgrade>();
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
        
        var job = new GatlingUpgradeJob
        {
            ECB = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
        };
        job.ScheduleParallel();
    }
}

[BurstCompile]
[WithAll(typeof(GatlingUpgrade))]
public partial struct GatlingUpgradeJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ECB;

    private void Execute(Entity entity, RefRW<UnitAttack> unitAttack, [ChunkIndexInQuery] int chunkIndex)
    {
        var dps = unitAttack.ValueRO.Strength / unitAttack.ValueRO.RateOfFire;
        unitAttack.ValueRW.RateOfFire = 0.33f;
        unitAttack.ValueRW.Strength = dps * 0.33f;
        ECB.RemoveComponent<GatlingUpgrade>(chunkIndex, entity);
    }
}

public struct GatlingUpgrade : IComponentData {}