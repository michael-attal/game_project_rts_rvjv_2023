using Unity.Burst;
using Unity.Entities;

partial struct ArtilleryUpgradeSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<UnitAttack>();
        state.RequireForUpdate<UnitDamage>();
        state.RequireForUpdate<ArtilleryUpgrade>();
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
        
        var job = new ArtilleryUpgradeJob
        {
            ECB = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
        };
        job.ScheduleParallel();
    }
}

[BurstCompile]
[WithAll(typeof(ArtilleryUpgrade))]
public partial struct ArtilleryUpgradeJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ECB;
    private void Execute(Entity entity, RefRW<Unit> unitMovement, RefRW<UnitAttack> unitAttack, [ChunkIndexInQuery] int chunkIndex)
    {
        unitAttack.ValueRW.Range = 20;
        unitMovement.ValueRW.UnitSpeed /= 2;
        
        ECB.RemoveComponent<ArtilleryUpgrade>(chunkIndex, entity);
    }
}

public struct ArtilleryUpgrade : IComponentData {}