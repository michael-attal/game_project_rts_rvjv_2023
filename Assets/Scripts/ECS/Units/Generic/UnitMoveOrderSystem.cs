using AnimCooker;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateAfter(typeof(MouseSystemGroup))] // We need to know if a mouse event occurred before updating this system
internal partial struct UnitMoveOrderSystem : ISystem
{
    private int lastClickID;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<MouseManager>();
        state.RequireForUpdate<UnitSelectable>();
        state.RequireForUpdate<MouseRightClickEvent>();

        lastClickID = -1;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var configManager = SystemAPI.GetSingleton<Config>();
        var mouseManagerEntity = SystemAPI.GetSingletonEntity<MouseManager>();

        if (!configManager.ActivateUnitMovementSystem)
        {
            state.Enabled = false;
            return;
        }

        // Get the mouse click event
        var mouseRightClickEventData = state.EntityManager.GetComponentData<MouseRightClickEvent>(mouseManagerEntity);

        var isNewClickEventDetected = lastClickID != mouseRightClickEventData.RightClickID;

        if (!isNewClickEventDetected)
            return;

        foreach (var (unit, unitMovement, entity) in SystemAPI
                     .Query<RefRO<Unit>, RefRO<UnitMovement>>()
                     .WithAll<UnitSelected>()
                     .WithEntityAccess())
        {
            ecb.SetComponentEnabled<WantsToMove>(entity, true);
            ecb.SetComponent(entity, new WantsToMove
            {
                Destination = mouseRightClickEventData.Position
            });

            if (SystemAPI.HasComponent<DestinationReached>(entity))
                ecb.RemoveComponent<DestinationReached>(entity);

            // NOTE: Start move animation
            ecb.SetComponent(entity, new AnimationCmdData
            {
                Cmd = AnimationCmd.SetPlayForever, ClipIndex = (short)AnimationsType.Move
            });
            ecb.SetComponent(entity, new AnimationSpeedData
            {
                PlaySpeed = unitMovement.ValueRO.Speed
            });
        }

        lastClickID = mouseRightClickEventData.RightClickID;
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}