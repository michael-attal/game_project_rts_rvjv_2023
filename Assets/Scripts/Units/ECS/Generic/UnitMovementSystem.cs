using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(TransformSystemGroup))]
// [UpdateAfter(typeof(UnitSelectableSystem))] // NOTE: We need to update after the to not lose the unit selection on the right click
public partial struct UnitMovementSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Unit>();
        state.RequireForUpdate<UnitSelectable>();
        state.RequireForUpdate<UnitMovement>();
    }

    public void OnUpdate(ref SystemState state)
    {
        // Implement the shared movement system here.
        // If the movement system differs significantly between units, we should implement a specialized system, such as MySlimeUnitMovementSystem, in addition of a generic one like this one.

        if (!Input.GetMouseButtonDown(1))
            return;

        var clickPos = Input.mousePosition;
        var mainCamera = Camera.main;
        var clickWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(clickPos.x, clickPos.y, mainCamera.transform.position.y));

        foreach (var (unitMovementLTW, unitSelectable)
                 in
                 SystemAPI
                     .Query<RefRW<LocalToWorld>, RefRO<UnitSelectable>>()
                     .WithAll<UnitMovement>())
        {
            // TODO: Create a destination float3 data within the UnitMovement component, which will store the intended world position to travel to. Subsequently, assign a velocity to the selected units, and ultimately eliminate the velocity prior to arrival. 
            // TODO: Avoid clustering all units together by spacing them out at a fixed distance from each other.
            if (unitSelectable.ValueRO.IsSelected)
            {
                var newPosition = new float3(
                    // mousePos.x,
                    clickWorldPosition.x,
                    clickWorldPosition.y,
                    // mousePos.z
                    clickWorldPosition.z
                );

                unitMovementLTW.ValueRW.Value =
                    float4x4.TRS(newPosition, quaternion.identity, unitMovementLTW.ValueRO.Value.Scale());
            }
        }

        Debug.Log("Unit selected moved!");
    }
}