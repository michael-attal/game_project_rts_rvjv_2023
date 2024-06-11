using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[UpdateBefore(typeof(TransformSystemGroup))]
[BurstCompile]
internal partial struct ParticleVelocitySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();

        if (!configManager.ActivateParticleSystems)
        {
            return;
        }

        if (configManager.IsGamePaused)
            return;

        var dt = SystemAPI.Time.DeltaTime;

        foreach (var (velocity, localTransform) in SystemAPI.Query<RefRO<ParticleVelocity>, RefRW<LocalTransform>>())
        {
            var translate = velocity.ValueRO.Value * dt;
            localTransform.ValueRW.Position += translate;
        }
    }
}