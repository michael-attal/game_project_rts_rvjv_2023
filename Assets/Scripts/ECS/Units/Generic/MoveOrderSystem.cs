using AnimCooker;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[UpdateAfter(typeof(MouseSystemGroup))] // We need to know if a mouse event occurred before updating this system
[UpdateBefore(typeof(MovementVelocity))]
[UpdateBefore(typeof(MovementPositionMotor))]
[UpdateBefore(typeof(MovementManualSystem))]
internal partial struct MoveOrderSystem : ISystem
{
    private int lastClickID;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<MouseManager>();
        state.RequireForUpdate<MouseRightClickEvent>();

        lastClickID = -1;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var configManager = SystemAPI.GetSingleton<Config>();
        var mouseManagerEntity = SystemAPI.GetSingletonEntity<MouseManager>();

        if (!configManager.ActivateMoveOrderSystem)
        {
            state.Enabled = false;
            return;
        }

        if (configManager.IsGamePaused)
            return;

        // Get the mouse click event
        var mouseRightClickEventData = state.EntityManager.GetComponentData<MouseRightClickEvent>(mouseManagerEntity);

        var isNewClickEventDetected = lastClickID != mouseRightClickEventData.RightClickID;

        if (!isNewClickEventDetected)
            return;

        // Process entities with standard MovementVelocity
        foreach (var (movementVelocity, entity) in SystemAPI
                     .Query<RefRO<MovementVelocity>>()
                     .WithAny<Selected>() // NOTE: More generic Selected component
                     .WithAny<UnitSelected>()
                     .WithAny<BuildingSelected>() // NOTE: In the case of allowing the movement of a building, such as in Warcraft 3 for elven buildings.
                     .WithEntityAccess())
        {
            HandleMovement(ecb, entity, mouseRightClickEventData.Position, SystemAPI.HasComponent<Unit>(entity), movementVelocity.ValueRO.Speed, movementVelocity.ValueRO.IsMovementAnimated);
        }

        // Process entities with MovementPositionMotor
        foreach (var (movementPositionMotor, entity) in SystemAPI
                     .Query<RefRO<MovementPositionMotor>>()
                     .WithAny<Selected>()
                     .WithAny<UnitSelected>()
                     .WithAny<BuildingSelected>()
                     .WithEntityAccess())
        {
            HandleMovement(ecb, entity, mouseRightClickEventData.Position, SystemAPI.HasComponent<Unit>(entity), movementPositionMotor.ValueRO.Speed, movementPositionMotor.ValueRO.IsMovementAnimated);
        }

        // Process entities with manual movement
        foreach (var (unitMovement, entity) in SystemAPI
                     .Query<RefRO<MovementManual>>()
                     .WithAny<Selected>()
                     .WithAny<UnitSelected>()
                     .WithAny<BuildingSelected>()
                     .WithEntityAccess())
        {
            HandleMovement(ecb, entity, mouseRightClickEventData.Position, SystemAPI.HasComponent<Unit>(entity), unitMovement.ValueRO.Speed, unitMovement.ValueRO.IsMovementAnimated);
        }

        // NOTE: Add  any other movement type here.

        lastClickID = mouseRightClickEventData.RightClickID;
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    private void HandleMovement(EntityCommandBuffer ecb, Entity entity, float3 destination, bool isUnit, float speed, bool isMovementAnimated)
    {
        ecb.SetComponentEnabled<WantsToMove>(entity, true);
        ecb.SetComponent(entity, new WantsToMove
        {
            Destination = destination
        });

        if (isUnit)
            ecb.SetComponentEnabled<UnitInMovementTag>(entity, true);

        if (isMovementAnimated)
        {
            // NOTE: Start move animation
            ecb.SetComponent(entity, new AnimationCmdData
            {
                Cmd = AnimationCmd.SetPlayForever, ClipIndex = (short)AnimationsType.Move
            });
            ecb.SetComponent(entity, new AnimationSpeedData
            {
                PlaySpeed = speed
            });
        }
    }
}