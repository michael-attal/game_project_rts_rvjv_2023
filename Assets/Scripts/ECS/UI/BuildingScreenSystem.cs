using ECS.UI;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

internal partial struct BuildingScreenSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<Game>();
    }

    // Accessing BuildingScreen, can't use BurstCompile
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();
        var gameManager = SystemAPI.GetSingleton<Game>();

        if (!configManager.ActivateBuildingScreenSystem)
        {
            state.Enabled = false;
            return;
        }

        if (gameManager.State == GameState.Paused)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        var clickPos = Input.mousePosition;
        var mainCamera = Camera.main;
        if (mainCamera == null)
            return;
        var clickWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(clickPos.x, clickPos.y, mainCamera.transform.position.y)); // TODO: Maybe do a calculation with the camera rotation instead of using y axis.

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var selectedID = BuildingScreenSingleton.Instance.SelectedPrefabID;
        var buffer = SystemAPI.GetBuffer<InstantiatableEntityData>(SystemAPI.GetSingletonEntity<Game>());
        for (var i = 0; i < buffer.Length; ++i)
        {
            if (buffer[i].EntityID == selectedID)
            {
                // Hard-coded cost because I'm tired
                if (gameManager.RessourceCount < 50)
                    return;

                gameManager.RessourceCount -= 50;
                SystemAPI.SetSingleton(gameManager);

                var newEntity = ecb.Instantiate(buffer[i].Entity);
                ecb.SetComponent(newEntity, new LocalTransform
                {
                    Position = clickWorldPosition,
                    Rotation = quaternion.identity,
                    Scale = 1f
                });
            }
        }

        BuildingScreenSingleton.Instance.ResetSelection();

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}