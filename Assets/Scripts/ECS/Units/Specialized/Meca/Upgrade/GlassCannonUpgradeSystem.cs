using Unity.Burst;
using Unity.Entities;

partial struct GlassCannonUpgradeSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<UnitAttack>();
        state.RequireForUpdate<UnitDamage>();
        state.RequireForUpdate<GlassCannonUpgrade>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var job = new GlassCannonUpgradeJob
        {
            ECB = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
        };
        job.ScheduleParallel();
    }
}

[BurstCompile]
[WithAll(typeof(GlassCannonUpgrade))]
public partial struct GlassCannonUpgradeJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ECB;

    private void Execute(Entity entity, RefRW<UnitDamage> unitDamage, RefRW<UnitAttack> unitAttack, [ChunkIndexInQuery] int chunkIndex)
    {
        unitAttack.ValueRW.Strength *= 2;
        unitDamage.ValueRW.Health /= 2;
        ECB.RemoveComponent<GlassCannonUpgrade>(chunkIndex, entity);
    }
}

public struct GlassCannonUpgrade : IComponentData {}