using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct PlayerSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnManager>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // We only want to spawn players one time. Disabling the system stops subsequent updates.
        state.Enabled = false;

        var spawnManager = SystemAPI.GetSingleton<SpawnManager>();

        // Instantiate an empty entity for each player.
        var playerOne = state.EntityManager.Instantiate(spawnManager.PlayerOneSpecies == Species.Slime
            ? spawnManager.SlimePlayerHandPrefab
            : spawnManager.MecaPlayerHandPrefab);
        state.EntityManager.SetComponentData(playerOne, new Player
        {
            PlayerNumber = 1,
            Species = spawnManager.PlayerOneSpecies,
            NbOfBaseSpawnerBuilding = spawnManager.NumberOfBaseSpawnerForPlayerOne,
            StartPosition = spawnManager.StartPositionBaseSpawnerPlayerOne
        });

        var playerTwo = state.EntityManager.Instantiate(spawnManager.PlayerTwoSpecies == Species.Slime
            ? spawnManager.SlimePlayerHandPrefab
            : spawnManager.MecaPlayerHandPrefab);
        state.EntityManager.SetComponentData(playerTwo, new Player
        {
            PlayerNumber = 2,
            Species = spawnManager.PlayerTwoSpecies,
            NbOfBaseSpawnerBuilding = spawnManager.NumberOfBaseSpawnerForPlayerTwo,
            StartPosition = spawnManager.StartPositionBaseSpawnerPlayerTwo
        });

        Debug.Log("Players successfully created!");
    }
}