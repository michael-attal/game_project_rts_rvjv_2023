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
        state.RequireForUpdate<Game>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();
        var gameManager = SystemAPI.GetSingleton<Game>();

        if (!configManager.ActivateParticleSystems)
        {
            return;
        }

        if (gameManager.State == GameState.Paused)
            return;

        var dt = SystemAPI.Time.DeltaTime;

        foreach (var (velocity, localTransform) in SystemAPI.Query<RefRO<ParticleVelocity>, RefRW<LocalTransform>>())
        {
            var translate = velocity.ValueRO.Value * dt;
            localTransform.ValueRW.Position += translate;
        }
    }
}