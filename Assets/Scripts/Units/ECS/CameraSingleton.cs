using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class CameraSingleton : MonoBehaviour
{
    public static Camera Instance;

    private void Awake()
    {
        Instance = GetComponent<Camera>();
        Debug.Log("CameSingleton created!");
    }

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

// Examples How to use (the example is old, we need to adapt it to 1.0 ecs) : https://forum.unity.com/threads/getting-camera-worldtoscreenpoint-player-translation-here-data-into-a-job.686539/#post-4599265
// public class PlayerMoveSystem : ISystem
// {
//     [BurstCompile]
//     struct PlayerMoveSystemJob : IJobForEach<InputComponent, MoveSpeedComponent, PhysicsVelocity, Rotation> {
//         public float DeltaTime;
//         public void Execute(ref InputComponent input,ref MoveSpeedComponent moveSpeed, ref PhysicsVelocity velocity, ref Rotation rotation) {
//             velocity.Linear.x = input.Horizontal * moveSpeed.MoveSpeed * 30 * DeltaTime;
//         }
//     }
//
//     protected override JobHandle OnUpdate(JobHandle inputDependencies)
//     {
//         var job = new PlayerMoveSystemJob {DeltaTime = Time.deltaTime};
//         return job.Schedule(this, inputDependencies);
//     }
// }
//
// public class AimSystem : ISystem
// {
//     [BurstCompile]
//     struct AimSystemJob : IJobForEach<AimComponent, Rotation, Translation>
//     {
//         public float3 MousePosition;
//         public float3 CameraPos;
//         public float4x4 CamProjMatrix;
//         public float3 CamUp;
//         public float3 CamRight;
//         public float3 CamForward;
//         public float PixelWidth;
//         public float PixelHeight;
//         public float ScaleFactor;
//         public void Execute(ref AimComponent aim, ref Rotation rotation, ref Translation translation) {
//             var dir = new float2(MousePosition.x, MousePosition.y)  - Helpers.ConvertWorldToScreenCoordinates(translation.Value, CameraPos, CamProjMatrix, CamUp, CamRight, CamForward, PixelWidth, PixelHeight, ScaleFactor);
//             var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
//             rotation.Value = Quaternion.AngleAxis(angle, Vector3.forward);
//         }
//     }
//
//     private Camera _camera;
//
//     protected override JobHandle OnUpdate(JobHandle inputDependencies)
//     {
//         if (_camera == null) _camera = Camera.main;
//
//         var job = new AimSystemJob
//         {
//             MousePosition = Input.mousePosition,
//             CameraPos = _camera.transform.position,
//             CamForward = _camera.transform.forward,
//             CamProjMatrix = _camera.projectionMatrix,
//             CamRight = _camera.transform.right,
//             CamUp = _camera.transform.up,
//             PixelHeight = _camera.pixelHeight,
//             PixelWidth = _camera.pixelWidth,
//             ScaleFactor = 1.5f
//         };
//
//         return job.Schedule(this, inputDependencies);
//     }
// }


// Another examples :
// public class CameraManager
// {
//     public static CameraManager instance { get; private set; }
//
//     public CameraSettings cameraSettings { get; private set; }
//
//     // Store the main camera entity or component.
//     public Camera mainCamera { get; private set; }
//
//     public CameraManager(Camera mainCamera)
//     {
//         if (instance != null)
//         {
//             throw new System.Exception("Attempted to create another instance of CameraManager.");
//         }
//         instance = this;
//
//         this.mainCamera = mainCamera;
//
//         cameraSettings = new CameraSettings
//         {
//             uiDepth = 0.5f // Example value for UI depth.
//         };
//     }
//
//     // Other camera-related methods can be added here.
// }
// Usage example:
// var mainCamera = CameraManager.instance.mainCamera;
// [BurstCompile]
// protected override void OnUpdate()
// {
//     if (processQuery.IsEmpty)
//     {
//         return;
//     }
//     var uiDepth = CameraManager.instance.cameraSettings.uiDepth;
//     float4x4 projectionMatrix = CameraReferences.GetMainCamera(EntityManager).projectionMatrix;
//     var controllerEntities = controllersQuery.ToEntityArrayAsync(Allocator.TempJob, out var jobHandleA);
//     Dependency = JobHandle.CombineDependencies(Dependency, jobHandleA);
//     var controllers = GetComponentDataFromEntity<Controller>(true);
//     var cameraEntities = camerasQuery.ToEntityArrayAsync(Allocator.TempJob, out var jobHandleB);
//     Dependency = JobHandle.CombineDependencies(Dependency, jobHandleB);
//     var cameras = GetComponentDataFromEntity<Camera>(true);
//     var localToWorlds = GetComponentDataFromEntity<LocalToWorld>(true);
//     controllerEntities.Dispose();
//     cameraEntities.Dispose();
//     Dependency = Entities.ForEach((ref Translation position, in CharacterLink characterLink, in FollowingMouse followingMouse) =>
//     {
//         var cameraEntity = followingMouse.camera;
//         var camera = cameras[cameraEntity];
//         var controller = controllers[characterLink.character];
//         var mousePosition = controller.mouseData.GetPointer(camera.screenDimensions.ToFloat2());    // Gives mouse position between 0 and 1 for camera.
//         if (controller.deviceType == DeviceType.Gamepad)
//         {
//             mousePosition = new float2(0.06f, 0.5f);    // gamepad will just show mouse on left side of screen
//         }
//         var screenPosition = new float3(mousePosition.x, mousePosition.y, uiDepth);
//         float4x4 cameraLocalToWorld = localToWorlds[cameraEntity].Value; // unityCamera.transform.localToWorldMatrix;
//         // Flip z for camera matrix
//         cameraLocalToWorld[2][0] *= -1;
//         cameraLocalToWorld[2][1] *= -1;
//         cameraLocalToWorld[2][2] *= -1;
//         position.Value = MouseFollowUISystem.ScreenToWorldPoint(screenPosition, cameraLocalToWorld, projectionMatrix);
//         // var unityCamera = EntityManager.GetSharedComponentData<GameObjectSynch>(cameraEntity).gameObject.GetComponent<UnityEngine.Camera>();
//         // float4x4 cameraToWorldMatrix = unityCamera.cameraToWorldMatrix;
//         // var unityPosition = unityCamera.ScreenToWorldPoint(new float3(controller.mouseData.pointer.x, controller.mouseData.pointer.y, uiDepth));
//         // position.Value = unityPosition;
//     })  .WithReadOnly(cameras)
//         .WithReadOnly(localToWorlds)
//         .WithReadOnly(controllers)
//         .ScheduleParallel(Dependency);
//     Dependency = Entities.ForEach((ref Rotation rotation, in FollowingMouse followingMouse) =>
//     {
//         rotation.Value = localToWorlds[followingMouse.camera].Rotation;
//     })  .WithReadOnly(localToWorlds)
//         .ScheduleParallel(Dependency);
// }
//
// public static float3 ScreenToWorldPoint(float3 screenPosition, float4x4 cameraToWorldMatrix, float4x4 projectionMatrix)
// {
//     var clipPosition = new float3((screenPosition.x * 2.0f) - 1.0f, (2.0f * screenPosition.y) - 1.0f, screenPosition.z);
//     var viewPosition = math.transform(math.inverse(projectionMatrix), clipPosition);
//     return math.transform(cameraToWorldMatrix, viewPosition);
// }


public struct ICCamera : IComponentData
{
}