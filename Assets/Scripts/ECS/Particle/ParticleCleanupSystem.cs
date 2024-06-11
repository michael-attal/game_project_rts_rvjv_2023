using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using ISystem = Unity.Entities.ISystem;
using SystemState = Unity.Entities.SystemState;

[BurstCompile]
internal partial struct ParticleCleanupSystem : ISystem
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

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        state.EntityManager.GetAllUniqueSharedComponents(out NativeList<ParticleParent> particleParents, Allocator.Temp);

        for (var i = 0; i < particleParents.Length; i++)
        {
            var query = SystemAPI.QueryBuilder().WithAll<ParticleParent>().WithNone<Particle>().Build();
            query.SetSharedComponentFilter(particleParents[i]);

            var count = query.CalculateEntityCount();
            ecb.RemoveComponent<ParticleParent>(query, EntityQueryCaptureMode.AtRecord);

            if (count > 0)
            {
                var parentParticleEntity = particleParents[i].Value;
                var generatorData = state.EntityManager.GetComponentData<ParticleGeneratorInfo>(parentParticleEntity);
                generatorData.ParticleCount -= count;
                ecb.SetComponent(parentParticleEntity, generatorData);
            }
        }

        particleParents.Dispose();

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}