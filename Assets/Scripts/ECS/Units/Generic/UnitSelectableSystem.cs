using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct UnitSelectableSystem : ISystem
{
    private const float minimumSelectionArea = 12f;
    private const float minimumSelectionAreaCenter = minimumSelectionArea / 2f;
    private bool isClicked;
    private Vector3 initialClickPosition;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<UnitSelectable>();
    }

    // TODO: Refactor with MouseManager singleton (see UnitMovementSystem).
    public void OnUpdate(ref SystemState state)
    {
        // Implement the shared unit selectable system here.
        // If the selectable system differs significantly between units, we should implement a specialized system, such as MySlimeUnitSelectableSystem, in addition of a generic one like this one.

        var configManager = SystemAPI.GetSingleton<Config>();

        if (!configManager.ActivateUnitSelectableSystem)
        {
            state.Enabled = false;
            return;
        }

        // Check if the left mouse button is clicked down
        if (Input.GetMouseButtonDown(0) && !isClicked)
        {
            isClicked = true;
            initialClickPosition = Input.mousePosition;
            Debug.Log(initialClickPosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isClicked = false;
            Debug.Log("Left click released, set unit selected now!");
            var mainCamera = Camera.main; // We get the main camera here. As a result, we are unable to burst compile this OnUpdate function. However, we  utilize a burst compile job below to iterate through the affected components, so everything is still performant.
            if (mainCamera == null) return;

            var finalClickPosition = Input.mousePosition;
            var left = Mathf.Min(initialClickPosition.x, finalClickPosition.x);
            var top = Mathf.Min(initialClickPosition.y, finalClickPosition.y);
            var width = Mathf.Abs(initialClickPosition.x - finalClickPosition.x);
            var height = Mathf.Abs(initialClickPosition.y - finalClickPosition.y);

            // NOTE: Incorporate a slight radius to enable unit selection with a single click.
            var selectionArea = new Rect(left - minimumSelectionAreaCenter, top - minimumSelectionAreaCenter,
                width + minimumSelectionArea, height + minimumSelectionArea);

            var transformCamera = mainCamera.transform;
            var unitSelectionJob = new UnitSelectionJob
            {
                CameraPos = transformCamera.position,
                CamForward = transformCamera.forward,
                CamProjMatrix = mainCamera.projectionMatrix,
                CamRight = transformCamera.right,
                CamUp = transformCamera.up,
                PixelHeight = mainCamera.pixelHeight,
                PixelWidth = mainCamera.pixelWidth,
                ScaleFactor = 1f,
                SelectionArea = selectionArea
            };
            unitSelectionJob.ScheduleParallel();
        }
    }
}

[WithAll(typeof(UnitSelectable))]
[BurstCompile]
public partial struct UnitSelectionJob : IJobEntity
{
    public float3 CameraPos;
    public float4x4 CamProjMatrix;
    public float3 CamUp;
    public float3 CamRight;
    public float3 CamForward;
    public float PixelWidth;
    public float PixelHeight;
    public float ScaleFactor;
    public Rect SelectionArea;

    // Because we want the global position of a child entity, we read LocalToWorld instead of LocalTransform.
    private void Execute(in LocalToWorld unitLT, RefRW<UnitSelectable> unitSelectable, RefRW<URPMaterialPropertyBaseColor> unitColor)
    {
        unitSelectable.ValueRW.IsSelected = false;
        unitColor.ValueRW.Value = unitSelectable.ValueRO.OriginalUnitColor; // TODO: Get back to the original color with UnitSelectableMaterialChangerSystem later.

        var unitRadius = unitLT.Value.Scale().x;

        var transformScreenPosition = CameraSingleton.ConvertWorldToScreenCoordinates(
            unitLT.Position,
            CameraPos,
            CamProjMatrix,
            CamUp,
            CamRight,
            CamForward,
            PixelWidth,
            PixelHeight,
            ScaleFactor // or unitRadius ?
        );

        // Add the unit radius to the selection
        var unitRect = new Rect(transformScreenPosition.x - unitRadius, transformScreenPosition.y - unitRadius,
            unitRadius * 2, unitRadius * 2);

        // Check if selection intersect with unit
        if (unitRect.Overlaps(SelectionArea, true))
        {
            unitSelectable.ValueRW.IsSelected = true;
            // Edit color to green. TODO: Later, add a green highlight to the existing color.
            unitColor.ValueRW.Value = new float4(0, 1, 0, 1);
        }
    }
}