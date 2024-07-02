using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct SelectionRectResizeSystem : ISystem
{
    private float defaultPlaneBoundSizeXZ;

    // NOTE: We can adjust the y value to get the selection above or below (e.g 0) the unit.
    private float positionSelectionRectY;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<Game>();
        state.RequireForUpdate<SelectionRect>();
        state.RequireForUpdate<SelectionRectResize>();
        state.RequireForUpdate<MouseManager>();
        state.RequireForUpdate<CameraManager>();

        // NOTE: The default size of the plane is 10 by 10 units. <- Please be cautious about updating it if the plane size changes. Alternatively, we can dynamically obtain the MeshRenderer.Bounds to adjust it, but this may impact performance.
        defaultPlaneBoundSizeXZ = 10f;
        positionSelectionRectY = 1f; // Set a minimum y (height) position to 1.
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();
        var gameManager = SystemAPI.GetSingleton<Game>();

        if (!configManager.ActivateSelectionRectResizeSystem)
        {
            state.Enabled = false;
            return;
        }

        if (gameManager.State == GameState.Paused)
            return;

        var mouseManagerEntity = SystemAPI.GetSingletonEntity<MouseManager>();
        var mouseManager = SystemAPI.GetComponent<MouseManager>(mouseManagerEntity);
        var mouseMovementEvent = SystemAPI.GetComponent<MouseMovementEvent>(mouseManagerEntity);

        if (!mouseManager.ActivateLeftClickEventSystem)
            return;

        // NOTE: Update the scale of the selection according to the mouse movement.
        if (mouseManager.IsLeftClickDown)
        {
            var initialWorldPosition = mouseManager.InitialClickPositionWorld;
            var currentWorldPosition = mouseMovementEvent.PositionWorld;

            var scale = new float3
            {
                x = math.abs(currentWorldPosition.x - initialWorldPosition.x) / defaultPlaneBoundSizeXZ,
                y = 1f,
                z = math.abs(currentWorldPosition.z - initialWorldPosition.z) / defaultPlaneBoundSizeXZ
            };

            // NOTE: Calculate the bottom left corner of the rectangle
            var bottomLeft = new float3(
                math.min(initialWorldPosition.x, currentWorldPosition.x),
                positionSelectionRectY,
                math.min(initialWorldPosition.z, currentWorldPosition.z)
            );

            // NOTE: Calculate the center position based on the bottomLeft and scale
            var centerPosition = bottomLeft + new float3(scale.x, 0f, scale.z) * defaultPlaneBoundSizeXZ * 0.5f;

            // NOTE: Update entities with SelectionRect component
            foreach (var selectionRectLTW in SystemAPI.Query<RefRW<LocalToWorld>>().WithAll<SelectionRect>())
            {
                selectionRectLTW.ValueRW.Value = float4x4.TRS(centerPosition, quaternion.identity, scale);
            }
        }

        if (mouseManager.IsLeftClickUp)
        {
            // NOTE: Hide the selection rect on click released.
            foreach (var selectionRectLTW in SystemAPI.Query<RefRW<LocalToWorld>>().WithAll<SelectionRect>())
            {
                selectionRectLTW.ValueRW.Value = float4x4.zero;
            }
        }
    }
}