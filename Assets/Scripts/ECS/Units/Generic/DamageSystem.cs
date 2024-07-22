using Unity.Burst;
using Unity.Entities;

[UpdateAfter(typeof(UnitAttackSystem))]
[BurstCompile]
public partial struct DamageSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<Game>();
        state.RequireForUpdate<Damage>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();
        var gameManager = SystemAPI.GetSingleton<Game>();

        if (!configManager.ActivateDamageSystem)
        {
            state.Enabled = false;
            return;
        }

        if (gameManager.State == GameState.Paused)
            return;

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

        var job = new UnitDamageJob
        {
            ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
        };

        job.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct UnitDamageJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ECB;

    private void Execute(Entity entity, RefRO<Damage> entityDamage, [ChunkIndexInQuery] int chunkIndex)
    {
        if (entityDamage.ValueRO.Health <= 0)
            ECB.DestroyEntity(chunkIndex, entity);
    }
}