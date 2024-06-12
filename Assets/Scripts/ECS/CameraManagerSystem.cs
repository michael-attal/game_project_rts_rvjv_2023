using Unity.Burst;
using Unity.Entities;
using UnityEngine;

// This system must execute after the transform system has been updated to prevent the camera from experiencing a one-frame delay.
[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct CameraManagerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<CameraManager>();
    }

    // [BurstCompile] Cannot burst compile: Access to camera
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();
        if (configManager.ActivateCameraManagerSystem == false)
        {
            state.Enabled = false;
            return;
        }

        if (configManager.IsGamePaused)
            return;

        var camera = Camera.main;
        if (camera == null) return;


        // NOTE: Update every frame the CameraManager data component
        var cameraManager = SystemAPI.GetSingleton<CameraManager>();
        var cameraTransform = camera.transform;
        var cameraData = new CameraManager
        {
            Position = cameraTransform.position,
            ProjectionMatrix = camera.projectionMatrix,
            Up = cameraTransform.up,
            Right = cameraTransform.right,
            Forward = cameraTransform.forward,
            PixelWidth = camera.pixelWidth,
            PixelHeight = camera.pixelHeight,
            ScaleFactor = cameraManager.ScaleFactor
        };
        SystemAPI.SetSingleton(cameraData);
    }
}