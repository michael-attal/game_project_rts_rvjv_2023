using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(TransformSystemGroup))]
[UpdateAfter(typeof(MouseSystemGroup))] // We need to know if a mouse event occurred before updating this system
public partial struct UnitMovementSystem : ISystem
{
    private bool isClicked;
    private float3 worldClickPosition;
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

        // if (Math.Abs(mouseRightClickEventData.LastPosition.x - mouseRightClickEventData.Position.x) < 0.01f &&
        //     Math.Abs(mouseRightClickEventData.LastPosition.y - mouseRightClickEventData.Position.y) < 0.01f &&
        //     Math.Abs(mouseRightClickEventData.LastPosition.z - mouseRightClickEventData.Position.z) < 0.01f)
        // {
        //     // Do something ?
        // }

        // If no units are moving and there's no right click event, don't do anything in this frame
        if (isUnitsMoving == false && isNewClickEventDetected == false)
        {
            return;
        }

        lastClickID = mouseRightClickEventData.RightClickID;
        var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

        var unitMovementJob = new UnitMovementJob
        {
            ECB = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
            DeltaTime = SystemAPI.Time.DeltaTime,
            Destination = mouseRightClickEventData.Position
        };

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

    private void Execute(Entity entity, RefRO<UnitSelectable> unitSelectable, RefRW<UnitMovement> unitMovement, RefRW<LocalTransform> transform, [ChunkIndexInQuery] int chunkIndex)
    {
        // TODO: Ajouter le click id en tant que paramètre. Si un nouveau clic est effectué avec les mêmes unités sélectionnées, la destination est remplacée. Sinon, elle reste inchangée.
        if (unitSelectable.ValueRO.IsSelected == false && unitMovement.ValueRO.IsMoving == false)
            return;

        var direction = math.normalize(Destination - transform.ValueRO.Position);

        var gravity = new float3(0.0f, -9.82f, 0.0f);

        // Only update the destination if the unit is not already moving
        if (!unitMovement.ValueRO.IsMoving)
        {
            unitMovement.ValueRW.Destination = Destination;
        }

        var distanceToDestination = math.distance(transform.ValueRO.Position, Destination);
        if (distanceToDestination < 0.1f)
        {
            unitMovement.ValueRW.IsMoving = false;
            unitMovement.ValueRW.Velocity = 0;
            ECB.RemoveComponent<IsMovingTag>(chunkIndex, entity);
        }
        else
        {
            unitMovement.ValueRW.IsMoving = true;
            transform.ValueRW.Position += direction * unitMovement.ValueRO.Speed * DeltaTime;
            unitMovement.ValueRW.Velocity += gravity * DeltaTime;
            ECB.AddComponent<IsMovingTag>(chunkIndex, entity);
        }

        var speed = math.lengthsq(unitMovement.ValueRO.Velocity);
        if (speed < 0.1f)
        {
            unitMovement.ValueRW.Velocity = 0;
        }
    }
}