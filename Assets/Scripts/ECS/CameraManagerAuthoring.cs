using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public class CameraManagerAuthoring : MonoBehaviour
{
    [SerializeField] private float cameraScaleFactor = 1.0f;

    private class Baker : Baker<CameraManagerAuthoring>
    {
        public override void Bake(CameraManagerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new CameraManager // NOTE: CameraManagerSystem will set other camera manager data at the first frame
            {
                ScaleFactor = authoring.cameraScaleFactor
            });
        }
    }
}

public class CameraManagerTools : MonoBehaviour
{
    // NOTE: Thanks to : https://forum.unity.com/threads/getting-camera-worldtoscreenpoint-player-translation-here-data-into-a-job.686539/#post-4594918
    /// <summary>
    ///     Convert point from world space to screen space
    /// </summary>
    /// <param name="point">Point in World Space</param>
    /// <param name="cameraPos">Camera position in World Space</param>
    /// <param name="camProjMatrix">Camera.projectionMatrix</param>
    /// <param name="camUp">Camera.transform.up</param>
    /// <param name="camRight">Camera.transform.right</param>
    /// <param name="camForward">Camera.transform.forward</param>
    /// <param name="pixelWidth">Camera.pixelWidth</param>
    /// <param name="pixelHeight">Camera.pixelHeight</param>
    /// <param name="scaleFactor">Canvas.scaleFactor</param>
    /// <returns></returns>
    public static float2 ConvertWorldToScreenCoordinates(float3 point, float3 cameraPos, float4x4 camProjMatrix,
        float3 camUp, float3 camRight, float3 camForward, float pixelWidth, float pixelHeight, float scaleFactor)
    {
        /*
         * 1 convert P_world to P_camera
         */
        var pointInCameraCoodinates = ConvertWorldToCameraCoordinates(point, cameraPos, camUp, camRight, camForward);


        /*
         * 2 convert P_camera to P_clipped
         */
        var pointInClipCoordinates = math.mul(camProjMatrix, pointInCameraCoodinates);

        /*
         * 3 convert P_clipped to P_ndc
         * Normalized Device Coordinates
         */
        var pointInNdc = pointInClipCoordinates / pointInClipCoordinates.w;


        /*
         * 4 convert P_ndc to P_screen
         */
        float2 pointInScreenCoordinates;
        pointInScreenCoordinates.x = pixelWidth / 2.0f * (pointInNdc.x + 1);
        pointInScreenCoordinates.y = pixelHeight / 2.0f * (pointInNdc.y + 1);


        // return screencoordinates with canvas scale factor (if canvas coords required)
        return pointInScreenCoordinates / scaleFactor;
    }

    private static float4 ConvertWorldToCameraCoordinates(float3 point, float3 cameraPos, float3 camUp, float3 camRight,
        float3 camForward)
    {
        // translate the point by the negative camera-offset
        //and convert to Vector4
        var translatedPoint = new float4(point - cameraPos, 1f);

        // create transformation matrix
        var transformationMatrix = float4x4.identity;
        transformationMatrix.c0 = new float4(camRight.x, camUp.x, -camForward.x, 0);
        transformationMatrix.c1 = new float4(camRight.y, camUp.y, -camForward.y, 0);
        transformationMatrix.c2 = new float4(camRight.z, camUp.z, -camForward.z, 0);

        var transformedPoint = math.mul(transformationMatrix, translatedPoint);

        return transformedPoint;
    }
}


public struct CameraManager : IComponentData
{
    public float3 Position;
    public float4x4 ProjectionMatrix;
    public float3 Up;
    public float3 Right;
    public float3 Forward;
    public float PixelWidth;
    public float PixelHeight;
    public float ScaleFactor;
}