using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

// FIXME: DOESNT WORK ATM
[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct MovementPositionMotorSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<Game>();
        state.RequireForUpdate<MovementPositionMotor>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();
        var gameManager = SystemAPI.GetSingleton<Game>();

        if (!configManager.ActivateMovementPositionMotorSystem)
        {
            state.Enabled = false;
            return;
        }

        if (gameManager.State == GameState.Paused)
            return;

        var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var deltaTime = SystemAPI.Time.DeltaTime;

        var movementJob = new MovementPositionMotorJob
        {
            ECB = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
            DeltaTime = deltaTime
        };

        movementJob.ScheduleParallel();
    }
}

[WithAll(typeof(MovementPositionMotor), typeof(WantsToMove))]
[BurstCompile]
public partial struct MovementPositionMotorJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ECB;
    public float DeltaTime;

    private void Execute(Entity entity, RefRW<MovementPositionMotor> motor, RefRO<WantsToMove> moveIntent, RefRO<LocalTransform> transform, [ChunkIndexInQuery] int chunkIndex)
    {
        var currentPosition = transform.ValueRO.Position;
        var destination = new float3(moveIntent.ValueRO.Destination.x, currentPosition.y, moveIntent.ValueRO.Destination.z);
        var direction = new float3(destination.x - currentPosition.x, 0, destination.z - currentPosition.z);

        if (math.lengthsq(direction) < 1e-5f)
        {
            ECB.SetComponentEnabled<WantsToMove>(chunkIndex, entity, false);
            ECB.AddComponent<DestinationReached>(chunkIndex, entity);
            return;
        }

        direction = math.normalize(direction);
        var distanceToDestination = math.distance(new float2(currentPosition.x, currentPosition.z), new float2(destination.x, destination.z));

        if (distanceToDestination < 0.1f)
        {
            ECB.SetComponentEnabled<WantsToMove>(chunkIndex, entity, false);
            ECB.AddComponent<DestinationReached>(chunkIndex, entity);
        }
        else
        {
            var newPosition = currentPosition + direction * motor.ValueRO.Speed * DeltaTime;

            // NOTE: Ensure the new position doesn't change the Y coordinate
            newPosition.y = currentPosition.y;

            var joint = PhysicsJoint.CreatePrismatic(
                new BodyFrame
                {
                    Axis = direction,
                    PerpendicularAxis = math.cross(direction, new float3(0, 1, 0)),
                    Position = currentPosition
                },
                new BodyFrame
                {
                    Axis = direction,
                    PerpendicularAxis = math.cross(direction, new float3(0, 1, 0)),
                    Position = newPosition
                },
                new Math.FloatRange(0, 0) // NOTE: Limiting the range to zero to block rotation
            );

            // NOTE: Create the joint entity and add it to the system
            var jointEntity = ECB.CreateEntity(chunkIndex);
            ECB.AddComponent(chunkIndex, jointEntity, joint);
            ECB.AddComponent(chunkIndex, jointEntity, new PhysicsConstrainedBodyPair(entity, Entity.Null, false)); // NOTE: Assuming entity is the dynamic body and no collision between bodies
        }
    }
}


public struct MovementPositionMotor : IComponentData
{
    public float Speed;
    public bool IsMovementAnimated;
    public float3 AxisBlocked;
    public float3 PerpendicularAxis;
}