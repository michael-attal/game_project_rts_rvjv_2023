using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct ProjectileRendererSystem : ISystem
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

        if (!configManager.ActivateProjectileRendererSystem)
        {
            state.Enabled = false;
            return;
        }

        if (gameManager.State == GameState.Paused)
            return;

        var ecbProjectileSpawnJob = new EntityCommandBuffer(Allocator.TempJob);

        foreach (var (throwerTransform, throwerProjectileInfos, projectileDestination, entity) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<ThrowerProjectileInfo>, RefRO<WantsToThrowProjectile>>().WithEntityAccess())
        {
            ecbProjectileSpawnJob.SetComponentEnabled<WantsToThrowProjectile>(entity, false);

            // Calculates the forward direction of the launcher
            var forwardDirection = math.mul(throwerTransform.ValueRO.Rotation, new float3(0, 0, 1));

            var initialOffset = throwerProjectileInfos.ValueRO.ProjectileInitialPositionOffset;

            // NOTE: Adjust the projectile offset from the thrower rotation to avoid collision with itself
            // TODO: Maybe just disable collision for projectile ?
            var adjustedOffset = initialOffset + forwardDirection * 0.5f;

            var projectileSpawnJob = new ProjectileSpawnJob
            {
                ECB = ecbProjectileSpawnJob.AsParallelWriter(),
                ProjectilePrefab = throwerProjectileInfos.ValueRO.ProjectilePrefab,
                ProjectileInitialPosition = throwerTransform.ValueRO.Position + adjustedOffset,
                ProjectileInitialRotation = throwerTransform.ValueRO.Rotation,
                ProjectileInitialScale = throwerProjectileInfos.ValueRO.ProjectileScale,
                IsProjectileMovementAnimated = throwerProjectileInfos.ValueRO.IsProjectileAnimated,
                ProjectileSpeed = throwerProjectileInfos.ValueRO.Speed,
                ProjectileDestination = projectileDestination.ValueRO.Destination
            };

            var projectileSpawnJobHandler = projectileSpawnJob.Schedule((int)throwerProjectileInfos.ValueRO.Speed, 64, state.Dependency);
            state.Dependency = projectileSpawnJobHandler;

            projectileSpawnJobHandler.Complete();
        }


        foreach (var (projectile, entity) in SystemAPI.Query<RefRO<Projectile>>().WithAll<DestinationReached>().WithEntityAccess())
        {
            // var projectileDestroyJob = new ProjectileDestroyJob
            // {
            //     ECB = new EntityCommandBuffer(Allocator.TempJob).AsParallelWriter()
            // };
            // projectileDestroyJob.ScheduleParallel();
            ecbProjectileSpawnJob.DestroyEntity(entity);
        }

        ecbProjectileSpawnJob.Playback(state.EntityManager);
        ecbProjectileSpawnJob.Dispose();
    }
}


[BurstCompile]
public struct ProjectileSpawnJob : IJobParallelFor
{
    public EntityCommandBuffer.ParallelWriter ECB;
    public Entity ProjectilePrefab;
    public float3 ProjectileInitialPosition;
    public Quaternion ProjectileInitialRotation;
    public float ProjectileInitialScale;
    public bool IsProjectileMovementAnimated;
    public float ProjectileSpeed;
    public float3 ProjectileDestination;

    public void Execute(int index)
    {
        var projectileInstance = ECB.Instantiate(index, ProjectilePrefab);

        ECB.AddComponent<Projectile>(index, projectileInstance);

        ECB.AddComponent(index, projectileInstance, new LocalTransform
        {
            Position = ProjectileInitialPosition,
            Rotation = ProjectileInitialRotation,
            Scale = ProjectileInitialScale
        });

        ECB.AddComponent(index, projectileInstance, new MovementManual
        {
            Speed = ProjectileSpeed,
            IsMovementAnimated = IsProjectileMovementAnimated
        });

        ECB.AddComponent(index, projectileInstance, new WantsToMove
        {
            Destination = ProjectileDestination
        });
        ECB.SetComponentEnabled<WantsToMove>(index, projectileInstance, true);
    }
}

[WithAll(typeof(Projectile), typeof(DestinationReached))]
[BurstCompile]
public partial struct ProjectileDestroyJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ECB;

    public void Execute(Entity entity, [ChunkIndexInQuery] int chunkIndex)
    {
        ECB.DestroyEntity(chunkIndex, entity);
    }
}


public struct Projectile : IComponentData
{
}

public struct ThrowerProjectileInfo : IComponentData
{
    public float Speed;
    public Entity ProjectilePrefab;
    public float ProjectileScale;
    public float3 ProjectileInitialPositionOffset;
    public bool IsProjectileAnimated;
}

public struct WantsToThrowProjectile : IComponentData, IEnableableComponent
{
    public float3 Destination;
}