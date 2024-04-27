using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

// TODO: Use Position Motor from Unity Physics instead
[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
[UpdateAfter(typeof(MouseSystemGroup))] // We need to know if a mouse event occurred before updating this system
public partial struct UnitMovementSystem : ISystem
{
    private EntityQuery isMovingTagQuery;
    private int lastClickID;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<MouseManager>();
        state.RequireForUpdate<MouseRightClickEvent>();
        state.RequireForUpdate<UnitSelectable>();
        state.RequireForUpdate<UnitMovement>();

        isMovingTagQuery = state.GetEntityQuery(ComponentType.ReadOnly<IsMovingTag>());
        lastClickID = -1;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Implement the shared movement system here.
        // If the movement system differs significantly between units, we should implement a specialized system, such as MySlimeUnitMovementSystem, in addition of a generic one like this one.
        var configManager = SystemAPI.GetSingleton<Config>();
        var mouseManagerEntity = SystemAPI.GetSingletonEntity<MouseManager>();

        if (!configManager.ActivateUnitMovementSystem)
        {
            state.Enabled = false;
            return;
        }

        // Check if any unit is moving
        var isUnitsMoving = !isMovingTagQuery.IsEmptyIgnoreFilter;
        var isNewClickEventDetected = true;

        // Get the mouse click event
        var mouseRightClickEventData = state.EntityManager.GetComponentData<MouseRightClickEvent>(mouseManagerEntity);

        if (lastClickID == mouseRightClickEventData.RightClickID)
        {
            isNewClickEventDetected = false;
        }

        // If no units are moving and there's no right click event, don't do anything in this frame
        if (isUnitsMoving == false && isNewClickEventDetected == false)
        {
            return;
        }

        var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

        var unitMovementJob = new UnitMovementJob
        {
            ECB = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
            DeltaTime = SystemAPI.Time.DeltaTime,
            Destination = mouseRightClickEventData.Position,
            IsNewDestination = lastClickID != mouseRightClickEventData.RightClickID
        };

        lastClickID = mouseRightClickEventData.RightClickID;
        unitMovementJob.ScheduleParallel();
    }
}

[WithAll(typeof(UnitMovement), typeof(UnitSelectable))]
[BurstCompile]
public partial struct UnitMovementJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ECB;
    public float DeltaTime;
    public float3 Destination;
    public bool IsNewDestination;

    private void Execute(Entity entity, RefRO<UnitSelectable> unitSelectable, RefRW<UnitMovement> unitMovement, RefRW<LocalTransform> transform, RefRW<PhysicsVelocity> velocity, [ChunkIndexInQuery] int chunkIndex)
    {
        if ((unitSelectable.ValueRO.IsSelected && IsNewDestination) || unitMovement.ValueRO.IsMoving)
        {
            // Update the destination only when a new destination is set while the unit is selected
            if (unitSelectable.ValueRO.IsSelected && IsNewDestination)
            {
                Destination = new float3(Destination.x, transform.ValueRO.Position.y, Destination.z); // Due to the camera coordinate issue with the y axis, we use the default y value for the specific type of unit being used (such as flying or underground units) for the moment.
                // NOTE: We should add [EntityIndexInQuery] int entityInQueryIndex in the Execute argument, and use it to create formationOffset. However, it seems that the entityInQueryIndex is not consistent, so I stopped here.
                unitMovement.ValueRW.Destination = Destination;
            }
            
            var direction = math.normalize(unitMovement.ValueRO.Destination - transform.ValueRO.Position);

            var distanceToDestination = math.distance(transform.ValueRO.Position, unitMovement.ValueRO.Destination);
            if (distanceToDestination < math.length(velocity.ValueRO.Linear))
            {
                unitMovement.ValueRW.IsMoving = false;
                ECB.RemoveComponent<IsMovingTag>(chunkIndex, entity);
            }
            else
            {
                unitMovement.ValueRW.IsMoving = true;

                var currentVelocity2D = velocity.ValueRO.Linear;
                currentVelocity2D.y = 0;
                var direction2D = math.normalize(new float3(direction.x, 0, direction.z));
                
                var wantedVelocity = direction2D * unitMovement.ValueRO.Speed;
                var remainingVelocity = wantedVelocity - currentVelocity2D;
                velocity.ValueRW.Linear += remainingVelocity;
                
                ECB.AddComponent<IsMovingTag>(chunkIndex, entity);
            }
        }
    }
}