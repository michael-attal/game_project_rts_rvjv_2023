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
    private Vector3 movement;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SelectionRect>();
        state.RequireForUpdate<SelectionRectResize>();
    }

    // TODO: Refactor the whole system to get better selection behaviour.
    // [BurstCompile] // TODO: Get the camera in parameter with a job to enable burst compile
    public void OnUpdate(ref SystemState state)
    {
        // Check if the left mouse button is clicked down
        if (Input.GetMouseButtonDown(0) && !isClicked)
        {
            isClicked = true;
            initialClickPosition = Input.mousePosition;
            initialClickPosition.z = Camera.main.transform.position.z;

            // TODO: Make it works.
            // // Convert screen coordinates to world coordinates
            // var worldPosition = Camera.main.ScreenToWorldPoint(initialClickPosition);
            //
            // foreach (var (selectionRectLocalToWorld, selectionRectEntity)
            //          in
            //          SystemAPI
            //              .Query<RefRW<LocalToWorld>>()
            //              .WithAll<SelectionRect>()
            //              .WithEntityAccess())
            // {
            //     var ltw = state.EntityManager.GetComponentData<LocalToWorld>(selectionRectEntity);
            //     // Set the initial position of the SelectionRect
            //     state.EntityManager.SetComponentData(selectionRectEntity, new LocalToWorld
            //     {
            //         Value = float4x4.TRS(
            //             new float3
            //             {
            //                 x = worldPosition.x,
            //                 y = ltw.Position.y,
            //                 z = worldPosition.z
            //             },
            //             ltw.Rotation,
            //             new float3
            //                 { x = 1.0f, y = 1.0f, z = 1.0f } // Set back to the original scale
            //         )
            //     });
            // }
        }

        // Update the scale of the selection according to the mouse movement.
        if (isClicked && Input.GetMouseButton(0))
        {
            var initialClickViewportPosition = Camera.main.ScreenToViewportPoint(initialClickPosition);
            var movementViewportPosition = Camera.main.ScreenToViewportPoint(movement);
            foreach (var (selectionRectLocalToWorld, selectionRectEntity)
                     in
                     SystemAPI
                         .Query<RefRW<LocalToWorld>>()
                         .WithAll<SelectionRect>()
                         .WithEntityAccess())
            {
                movement = Input.mousePosition;
                movement.z = Camera.main.transform.position.z;
                var ltw = state.EntityManager.GetComponentData<LocalToWorld>(selectionRectEntity);

                // Calculate scale based on mouse movement
                var scale = new float3
                {
                    x = Mathf.Abs(movementViewportPosition.x - initialClickViewportPosition.x) * 10,
                    y = 1,
                    z = Mathf.Abs(movementViewportPosition.y - initialClickViewportPosition.y) * 10
                };

                // // Convert viewport coordinates to world coordinates - NOT WORKING.
                // var initialClickWorldPosition = Camera.main.ViewportToWorldPoint(initialClickViewportPosition);
                // var movementWorldPosition = Camera.main.ViewportToWorldPoint(movementViewportPosition);
                //
                // // Calculate the new position based on mouse movement
                // var position = new float3
                // {
                //     x = Mathf.Min(initialClickWorldPosition.x, movementWorldPosition.x) + scale.x / 2,
                //     y = ltw.Position.y,
                //     z = Mathf.Min(initialClickWorldPosition.z, movementWorldPosition.z) + scale.z / 2
                // };
                var position = ltw.Position;

                state.EntityManager.SetComponentData(selectionRectEntity, new LocalToWorld
                {
                    Value = float4x4.TRS(
                        position,
                        ltw.Rotation,
                        scale
                    )
                });
            }
        }


        if (Input.GetMouseButtonUp(0))
        {
            isClicked = false;

            // Put selection rect scale back to 0. TODO: Create a method to do it and avoid duplicated code
            foreach (var (selectionRectLocalToWorld, selectionRectEntity)
                     in
                     SystemAPI
                         .Query<RefRW<LocalToWorld>>()
                         .WithAll<SelectionRect>()
                         .WithEntityAccess())
            {
                var ltw = state.EntityManager.GetComponentData<LocalToWorld>(selectionRectEntity);

                // Calculate scale based on mouse movement
                var newScale = new float3
                {
                    x = 0,
                    y = 0,
                    z = 0
                };

                state.EntityManager.SetComponentData(selectionRectEntity, new LocalToWorld
                {
                    Value = float4x4.TRS(
                        ltw.Position,
                        ltw.Rotation,
                        newScale
                    )
                });
            }
        }
    }
}