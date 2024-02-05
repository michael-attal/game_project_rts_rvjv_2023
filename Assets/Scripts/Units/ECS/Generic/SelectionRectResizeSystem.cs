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
        }

        // Update the scale of the selection according to the mouse movement.
        if (isClicked && Input.GetMouseButton(0))
        {
            movement = Input.mousePosition;
            movement.z = Camera.main.transform.position.z;

            var movementWorldPosition = Camera.main.ScreenToWorldPoint(movement);
            var initialClickWorldPosition = Camera.main.ScreenToWorldPoint(initialClickPosition);

            foreach (var selectionRectLTW
                     in
                     SystemAPI
                         .Query<RefRW<LocalToWorld>>()
                         .WithAll<SelectionRect>())
            {
                // NOTE: It works with camera at 40 on the y-axis and angle set to 75 - Not tested with other camera config
                // NOTE: The default size of the plane is 10 by 10 units. <- Please be cautious about updating it if the plane size changes. Alternatively, we can dynamically obtain the MeshRenderer.Bounds to adjust it, but this may impact performance.
                var defaultPlaneBoundSizeX = 10f;
                // We could use something like defaultPlaneBoundSizeXZ because the plane is a square.
                var defaultPlaneBoundSizeZ = 10f;
                var smallestUnitOfTheGameScaleXAdaptedToPlaneSize = 1f / defaultPlaneBoundSizeX;
                var smallestUnitOfTheGameScaleYAdaptedToPlaneSize = 1f / defaultPlaneBoundSizeZ;
                // TODO: I'm not sure why the mouse position is approximately three times smaller. I suppose it's because of the camera property. I need to inquire Sacha about the implementation of the camera.
                var
                    correctionMouse =
                        3.12f; // TODO: Faire un tableau de proportionnalité entre la position Y de la caméra et mes tests de position de clic sur l'écran afin d'obtenir une correction précise. Par la suite, examiner l'origine de cette corrélation.
                // Same for the correctionDeltaZ, I think the issue come from converting the mouse position from FPS view to RTS view.
                var correctionDeltaZ = 8f;

                // Calculate scale based on mouse movement
                var scale = new float3
                {
                    x = Mathf.Abs(initialClickWorldPosition.x - movementWorldPosition.x) * correctionMouse /
                        defaultPlaneBoundSizeX, // Probablement ajouter smallestUnitOfTheGameScaleYAdaptedToPlaneSize par la suite.
                    y = 1,
                    z = Mathf.Abs(initialClickWorldPosition.z - movementWorldPosition.z) * correctionMouse /
                        defaultPlaneBoundSizeX
                };

                var calcultatedBoundSizeXByScale = defaultPlaneBoundSizeX * scale.x;
                var calcultatedBoundSizeZByScale = defaultPlaneBoundSizeZ * scale.z;

                // Calculate the new position based on mouse movement
                var calculatedPositionX = 0f;

                // Invert because local to world is xyx inverted
                if (initialClickWorldPosition.x <= 0f)
                {
                    calculatedPositionX =
                        -(movementWorldPosition.x * correctionMouse) - calcultatedBoundSizeXByScale / 2;
                    if (initialClickWorldPosition.x < movementWorldPosition.x)
                        calculatedPositionX =
                            -(movementWorldPosition.x * correctionMouse) + calcultatedBoundSizeXByScale / 2;
                }
                else
                {
                    Debug.Log("ici");
                    // TODO: Continuer ici
                }

                var calculatedPositionY = 0f;

                if (initialClickWorldPosition.z + correctionDeltaZ <= 0f)
                {
                    // TODO: Faire les bons calculs ici
                    calculatedPositionY =
                        -movementWorldPosition.z - correctionDeltaZ - calcultatedBoundSizeZByScale / 2;
                }
                else
                {
                    Debug.Log("ici");
                    Debug.Log("ici");
                    // TODO: Continuer ici
                }

                var position = new float3
                {
                    // Invert x & y. Divide by 2 to get the middle
                    x = calculatedPositionX,
                    y = 10,
                    z = calculatedPositionY
                };

                Debug.Log("========= scaleX : " + scale.x);
                Debug.Log("========= scaleZ : " + scale.z);
                Debug.Log("========= positionX : " + position.x);
                Debug.Log("========= positionZ : " + position.z);
                Debug.Log("========= movementWorldPosition.x : " + movementWorldPosition.x);
                Debug.Log("========= movementWorldPosition.y : " + movementWorldPosition.y);
                Debug.Log("========= movementWorldPosition.z : " + movementWorldPosition.z);
                Debug.Log(
                    "========= mon calcul : " +
                    (calculatedPositionY =
                        -movementWorldPosition.z - correctionDeltaZ - calcultatedBoundSizeZByScale / 2));

                selectionRectLTW.ValueRW.Value = float4x4.TRS(
                    position,
                    selectionRectLTW.ValueRO.Rotation,
                    scale
                );
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
                selectionRectLTW.ValueRW.Value = float4x4.TRS(
                    selectionRectLTW.ValueRO.Position,
                    selectionRectLTW.ValueRO.Rotation,
                    new float3 { x = 0f, y = 0f, z = 0f }
                );
        }
    }
}