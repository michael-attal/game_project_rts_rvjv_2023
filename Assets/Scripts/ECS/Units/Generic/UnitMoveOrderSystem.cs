using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateAfter(typeof(MouseSystemGroup))] // We need to know if a mouse event occurred before updating this system
partial struct UnitMoveOrderSystem : ISystem
{
    private int lastClickID;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
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
        
        bool isNewClickEventDetected = lastClickID != mouseRightClickEventData.RightClickID;

        if (!isNewClickEventDetected)
            return;

        foreach (var (selectable, entity) in SystemAPI
                     .Query<RefRO<UnitSelectable>>()
                     .WithEntityAccess())
        {
            if (selectable.ValueRO.IsSelected)
            {
                ecb.SetComponentEnabled<WantsToMove>(entity, true);
                ecb.SetComponent(entity, new WantsToMove()
                {
                    Destination = mouseRightClickEventData.Position
                });
            }
        }
        
        lastClickID = mouseRightClickEventData.RightClickID;
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
