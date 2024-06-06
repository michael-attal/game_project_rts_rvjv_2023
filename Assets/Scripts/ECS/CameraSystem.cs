using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

// This system must execute after the transform system has been updated to prevent the camera from experiencing a one-frame delay.
[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct CameraSystem : ISystem
{
    private Entity target;
    private int numberOfPlayers;
    private int playerNumberFocused;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ICCamera>();
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<Player>();
        numberOfPlayers = 2; // TODO: If we allow more than 2 players, create spawnManager.NumberOfPlayer and update this code
        playerNumberFocused = 1;
    }

    // Because this OnUpdate accesses managed objects, it cannot be Burst-compiled.
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();
        if (configManager.ActivateSwitchFocusCameraToPlayersOnSpacePress == false)
        {
            state.Enabled = false;
            return;
        }

        if (configManager.IsGamePaused)
            return;

        // Change camera focus on enter to switch between players.
        if (!Input.GetKeyDown(KeyCode.Space))
            return;

        if (playerNumberFocused == numberOfPlayers + 1)
            playerNumberFocused = 1;


        var playerQuery = SystemAPI.QueryBuilder().WithAll<Player>().Build();
        var players = playerQuery.ToEntityArray(Allocator.Temp);
        if (players.Length == 0) return;
        target = players[playerNumberFocused - 1];

        var cameraTransform = CameraSingleton.Instance.transform;
        var playerTransform = SystemAPI.GetComponent<LocalToWorld>(target);
        Vector3 cameraPosition = playerTransform.Position;
        cameraPosition -=
            10.0f * (Vector3)playerTransform.Forward; // move the camera back from the player
        cameraPosition += new Vector3(0, 40f, 0); // raise the camera by an offset
        cameraTransform.position = cameraPosition;
        cameraTransform.LookAt(playerTransform.Position);

        playerNumberFocused++; // NOTE: Next time, focus the the next player
        Debug.Log("Focus to Player " + playerNumberFocused + " now!");
    }
}