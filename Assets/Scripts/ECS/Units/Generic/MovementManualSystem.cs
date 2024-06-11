using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(TransformSystemGroup))]
[BurstCompile]
public partial struct MovementManualSystem : ISystem
{
    private float deltaTime;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<MovementManual>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();

        if (!configManager.ActivateMovementManualSystem)
        {
            state.Enabled = false;
            return;
        }

        if (configManager.IsGamePaused)
            return;

        var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

        deltaTime = state.WorldUnmanaged.Time.DeltaTime;

        var unitMovementJob = new MovementManualSystemJob
        {
            ECB = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
            DeltaTime = deltaTime
        };

        state.Dependency = unitMovementJob.ScheduleParallel(state.Dependency);
    }
}

[WithAll(typeof(MovementManual), typeof(WantsToMove))]
[BurstCompile]
public partial struct MovementManualSystemJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ECB;
    public float DeltaTime;

    private void Execute(Entity entity, RefRW<MovementManual> movementManual, RefRO<WantsToMove> moveIntent, RefRW<LocalTransform> transform, [ChunkIndexInQuery] int chunkIndex)
    {
        var currentPosition = transform.ValueRO.Position;
        var destination = new float3(moveIntent.ValueRO.Destination.x, currentPosition.y, moveIntent.ValueRO.Destination.z);
        var direction = math.normalize(new float3(destination.x - currentPosition.x, 0, destination.z - currentPosition.z));
        var distanceToDestination = math.distance(new float2(currentPosition.x, currentPosition.z), new float2(destination.x, destination.z));

        const float epsilon = 0.01f;

        if (distanceToDestination < epsilon)
        {
            transform.ValueRW.Position = destination;
            ECB.SetComponentEnabled<WantsToMove>(chunkIndex, entity, false);
            ECB.AddComponent<DestinationReached>(chunkIndex, entity);
        }
        else
        {
            var moveDistance = movementManual.ValueRO.Speed * DeltaTime;
            if (moveDistance > distanceToDestination)
            {
                moveDistance = distanceToDestination;
            }

            var newPosition = currentPosition + direction * moveDistance;

            if (!math.all(newPosition == currentPosition))
            {
                transform.ValueRW.Position = newPosition;

                // NOTE: Only update rotation if the direction has changed significantly
                var targetRotation = quaternion.LookRotationSafe(direction, math.up());
                const float rotationThreshold = 0.001f;

                if (math.dot(transform.ValueRO.Rotation, targetRotation) < 1.0f - rotationThreshold)
                {
                    transform.ValueRW.Rotation = targetRotation;
                }
            }
        }
    }
}

public struct MovementManual : IComponentData
{
    public float Speed;
    public bool IsMovementAnimated;
}