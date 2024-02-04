using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct UnitSelectableSystem : ISystem
{
    private bool isClicked;
    private Vector3 initialClickPosition;
    private Vector3 movement;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Unit>();
        state.RequireForUpdate<UnitSelectable>();
    }

    // [BurstCompile] // TODO: Get the camera in parameter with a job to enable burst compile
    public void OnUpdate(ref SystemState state)
    {
        // TODO: Later use a UnitSelectionJob and do a job.ScheduleParallel to get even more performance.
        // Implement the shared unit selectable system here.
        // If the selectable system differs significantly between units, we should implement a specialized system, such as MySlimeUnitSelectableSystem, in addition of a generic one like this one.

        // Check if the left mouse button is clicked down
        if (Input.GetMouseButtonDown(0) && !isClicked)
        {
            isClicked = true;
            initialClickPosition = Input.mousePosition;
            Debug.Log(initialClickPosition);
        }

        if (isClicked && Input.GetMouseButton(0))
        {
            // TODO: Create a green selection rectangle while moving mouse.
        }

        if (Input.GetMouseButtonUp(0))
        {
            isClicked = false;
            Debug.Log("Left click released, set unit selected now!");
            var mainCamera = Camera.main; // Get the main camera // TODO: Use a job to allow Burst compile

            var finalClickPosition = Input.mousePosition;
            var left = Mathf.Min(initialClickPosition.x, finalClickPosition.x);
            var top = Mathf.Min(initialClickPosition.y, finalClickPosition.y);
            var width = Mathf.Abs(initialClickPosition.x - finalClickPosition.x);
            var height = Mathf.Abs(initialClickPosition.y - finalClickPosition.y);

            var selectionArea = new Rect(left, top, width, height);

            foreach (var (unitSelectableTransform, unitSelectableColor, unitSelectableComponent, unitSelectableEntity)
                     in
                     SystemAPI
                         .Query<RefRO<LocalTransform>, RefRW<URPMaterialPropertyBaseColor>, RefRW<UnitSelectable>>()
                         .WithAll<UnitSelectable>()
                         .WithEntityAccess())
            {
                unitSelectableComponent.ValueRW.IsSelected = false;
                unitSelectableColor.ValueRW.Value = new float4(0, 0, 255, 255); // TODO: Get back to the original color

                var unitRadius = unitSelectableTransform.ValueRO.Scale;
                Vector3 transformPosition = unitSelectableTransform.ValueRO.Position;

                var transformScreenPosition = mainCamera.WorldToScreenPoint(transformPosition);

                // Add the unit radius to the selection
                var unitRect = new Rect(transformScreenPosition.x - unitRadius, transformScreenPosition.y - unitRadius,
                    unitRadius * 2, unitRadius * 2);

                // Check if selection intersect with unit
                if (unitRect.Overlaps(selectionArea, true))
                {
                    unitSelectableComponent.ValueRW.IsSelected = true;
                    // Edit color to green. TODO: Later, add a green highlight to the existing color.
                    unitSelectableColor.ValueRW.Value = new float4(0, 255, 0, 255);
                }
            }
        }
    }
}