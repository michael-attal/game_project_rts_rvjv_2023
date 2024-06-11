using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using ISystem = Unity.Entities.ISystem;
using SystemState = Unity.Entities.SystemState;

[UpdateBefore(typeof(ParticleSpawningSystem))]
[BurstCompile]
internal partial struct ParticleGeneratorLifeSystem : ISystem
{
    [BurstCompile]
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
            state.Enabled = false;
            return;
        }

        if (configManager.IsGamePaused)
            return;

        var dt = SystemAPI.Time.DeltaTime;

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (particleGenerator, entity) in SystemAPI.Query<ParticleGeneratorAspect>().WithEntityAccess())
        {
            if (particleGenerator.LifetimeOfGenerator <= 0.001f)
            {
                // NOTE: Generator of particle will never expire
                continue;
            }

            particleGenerator.Age += dt;
            particleGenerator.AgeOverLifetime = math.saturate(particleGenerator.Age / particleGenerator.LifetimeOfGenerator);

            if (particleGenerator.Age >= particleGenerator.LifetimeOfGenerator)
            {
                if (particleGenerator.Rate <= 0.001f)
                {
                    // NOTE: If particle life has been set to forever, update it to 0.1 to allow it to be destroyed by the particle life system. 
                    if (particleGenerator.LifetimeOfParticle <= 0.001f)
                    {
                        particleGenerator.LifetimeOfParticle = 0.1f;
                    }

                    // NOTE: Wait until all Generator parent entities have destroyed their child particles.
                    if (particleGenerator.ParticleCount == 0)
                    {
                        ecb.DestroyEntity(entity);
                    }
                }
                else
                {
                    // NOTE: Set particleGenerator.Rate to 0 to cancel the production of particles and prepare for destruction.
                    particleGenerator.Rate = 0f;
                }
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}