using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct UnitDamageSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<UnitDamage>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Implement the shared damage system here.
        // If the damage system differs significantly between units, we should implement a specialized system, such as MySlimeUnitDamageSystem, in addition of a generic one like this one.

        var configManager = SystemAPI.GetSingleton<Config>();

        if (!configManager.ActivateUnitDamageSystem)
        {
            state.Enabled = false;
            return;
        }

        if (configManager.IsGamePaused)
            return;

        var job = new UnitDamageJob
        {
            ECB = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
        };
        job.ScheduleParallel();
    }
}

public partial struct UnitDamageJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ECB;

    private void Execute(Entity entity, RefRO<UnitDamage> unitDamage, [ChunkIndexInQuery] int chunkIndex)
    {
        if (unitDamage.ValueRO.Health <= 0)
            ECB.DestroyEntity(chunkIndex, entity);
    }
}