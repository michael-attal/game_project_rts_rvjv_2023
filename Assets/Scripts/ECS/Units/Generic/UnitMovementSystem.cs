using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct UnitMovementSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<UnitMovement>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // If the movement system differs significantly between units, we should implement a specialized system, such as MySlimeUnitMovementSystem, in addition of a generic one like this one.
        var configManager = SystemAPI.GetSingleton<Config>();

        if (!configManager.ActivateUnitMovementSystem)
        {
            state.Enabled = false;
            return;
        }

        var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

        var unitMovementJob = new UnitMovementJob
        {
            ECB = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
        };

        unitMovementJob.ScheduleParallel();
    }
}

[WithAll(typeof(UnitMovement), typeof(WantsToMove))]
[BurstCompile]
public partial struct UnitMovementJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ECB;

    private void Execute(Entity entity, RefRW<UnitMovement> unitMovement, RefRO<WantsToMove> moveIntent, RefRO<LocalTransform> transform, RefRW<PhysicsVelocity> velocity, [ChunkIndexInQuery] int chunkIndex)
    {
        var direction = math.normalize(moveIntent.ValueRO.Destination - transform.ValueRO.Position);
        var distanceToDestination = math.distance(transform.ValueRO.Position, moveIntent.ValueRO.Destination);

        if (distanceToDestination < math.length(velocity.ValueRO.Linear))
        {
            ECB.SetComponentEnabled<WantsToMove>(chunkIndex, entity, false);
            ECB.AddComponent<DestinationReached>(chunkIndex, entity);
        }
        else
        {
            var currentVelocity2D = velocity.ValueRO.Linear;
            currentVelocity2D.y = 0;
            var direction2D = math.normalize(new float3(direction.x, 0, direction.z));
            
            var wantedVelocity = direction2D * unitMovement.ValueRO.Speed;
            var remainingVelocity = wantedVelocity - currentVelocity2D;
            velocity.ValueRW.Linear += remainingVelocity;
        }
    }
}