using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct SelectionRectResizeSystem : ISystem
{
    private bool isClicked;
    private Vector3 initialClickPosition;
    private Vector3 initialWorldPosition;

    private float defaultPlaneBoundSizeXZ;

    // NOTE: We can adjust the y value to get the selection above or below (e.g 0) the unit.
    private float positionSelectionRectY;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<SelectionRect>();
        state.RequireForUpdate<SelectionRectResize>();
        // NOTE: The default size of the plane is 10 by 10 units. <- Please be cautious about updating it if the plane size changes. Alternatively, we can dynamically obtain the MeshRenderer.Bounds to adjust it, but this may impact performance.
        defaultPlaneBoundSizeXZ = 10f;
        positionSelectionRectY = 1f; // Set a minimum y (height) position to 1.
    }

    // TODO: In case there are additional selection rect entities (which seems unlikely), we should create a job that will adjust the position and size of the selection rect based on the mouse input received within this non-BurstCompile OnUpdate function to get additional performance.
    // TODO: Refactor with MouseManager singleton (see UnitMovementSystem).
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();

        if (!configManager.ActivateSelectionRectResizeSystem)
        {
            state.Enabled = false;
            return;
        }

        if (configManager.IsGamePaused)
            return;

        // Check if the left mouse button is clicked down
        if (Input.GetMouseButtonDown(0) && !isClicked)
        {
            var mainCamera = Camera.main;
            isClicked = true;
            initialClickPosition = Input.mousePosition;
            initialWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(initialClickPosition.x, initialClickPosition.y, mainCamera.transform.position.y)); // NOTE: Because we are in RTS view, we need to reverse the y and z axes.
        }

        // Update the scale of the selection according to the mouse movement.
        if (isClicked && Input.GetMouseButton(0))
        {
            var mainCamera = Camera.main;
            var positionCamera = mainCamera.transform.position;
            var currentMousePosition = Input.mousePosition;
            var currentWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(currentMousePosition.x, currentMousePosition.y, positionCamera.y));

            var scale = new float3
            {
                x = math.abs(currentWorldPosition.x - initialWorldPosition.x) / defaultPlaneBoundSizeXZ,
                y = 1f,
                z = math.abs(currentWorldPosition.z - initialWorldPosition.z) / defaultPlaneBoundSizeXZ
            };

            // TODO: We still need to address the slight discrepancy between the mouse position and the selectionRect position.
            // Calculate the bottom left corner of the rectangle
            var bottomLeft = new float3(
                math.min(initialWorldPosition.x, currentWorldPosition.x),
                positionSelectionRectY,
                math.min(initialWorldPosition.z, currentWorldPosition.z)
            );

            // Calculate the center position based on the bottomLeft and scale
            var centerPosition = bottomLeft + new float3(scale.x, 0f, scale.z) * 0.5f;

            // Adjust the center position based on the scale
            centerPosition.x += initialWorldPosition.x > currentWorldPosition.x
                ? -scale.x * -defaultPlaneBoundSizeXZ * 0.5f
                : scale.x * defaultPlaneBoundSizeXZ * 0.5f;
            centerPosition.z += initialWorldPosition.z > currentWorldPosition.z
                ? -scale.z * -defaultPlaneBoundSizeXZ * 0.5f
                : scale.z * defaultPlaneBoundSizeXZ * 0.5f;

            // Update entities with SelectionRect component
            foreach (var selectionRectLTW
                     in
                     SystemAPI
                         .Query<RefRW<LocalToWorld>>()
                         .WithAll<SelectionRect>())
            {
                selectionRectLTW.ValueRW.Value = float4x4.TRS(centerPosition, quaternion.identity, scale);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isClicked = false;
            // Hide the selection rect on click released.
            foreach (var selectionRectLTW
                     in
                     SystemAPI
                         .Query<RefRW<LocalToWorld>>()
                         .WithAll<SelectionRect>())
            {
                selectionRectLTW.ValueRW.Value = float4x4.zero;
            }
        }
    }
}