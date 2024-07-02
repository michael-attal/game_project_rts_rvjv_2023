using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using ISystem = Unity.Entities.ISystem;
using SystemState = Unity.Entities.SystemState;

internal partial struct ParticleLifeSystem : ISystem
{
    [BurstCompile]
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
            state.Enabled = false;
            return;
        }

        if (gameManager.State == GameState.Paused)
            return;

        var dt = SystemAPI.Time.DeltaTime;

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (particle, entity) in SystemAPI.Query<RefRW<Particle>>().WithEntityAccess())
        {
            if (particle.ValueRO.Lifetime <= 0.001f)
            {
                // NOTE: Particle will never expire
                continue;
            }

            particle.ValueRW.Age += dt;
            particle.ValueRW.AgeOverLifetime = math.saturate(particle.ValueRO.Age / particle.ValueRO.Lifetime);

            if (particle.ValueRO.Age >= particle.ValueRO.Lifetime)
            {
                ecb.DestroyEntity(entity);
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}