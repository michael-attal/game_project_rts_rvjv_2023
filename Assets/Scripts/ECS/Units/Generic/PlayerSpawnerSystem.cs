using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(TransformSystemGroup))]
[BurstCompile]
public partial struct PlayerSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<SpawnManager>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();

        if (!configManager.ActivatePlayerSpawnerSystem)
        {
            state.Enabled = false;
            return;
        }

        if (configManager.IsGamePaused)
            return;

        // We only want to spawn players one time. Disabling the system stops subsequent updates.
        state.Enabled = false;

        var spawnManager = SystemAPI.GetSingleton<SpawnManager>();

        // NOTE: If we allow more than 2 players, adjust the loop here
        // TODO: Use a job and settings from main menu to load players
        for (uint i = 1; i <= 2; i++)
        {
            // Instantiate the hand entity for each player (the hand will replace the cursor of the mouse for more immersion).
            var playerSpecies = i == 1
                ? spawnManager.PlayerOneSpecies
                : spawnManager.PlayerTwoSpecies;

            var playerHandPrefab = playerSpecies == SpeciesType.Slime
                ? spawnManager.SlimePlayerHandPrefab
                : spawnManager.MecaPlayerHandPrefab;

            var baseSpawnerBuildingPrefab = playerSpecies == SpeciesType.Slime
                ? spawnManager.SlimeBaseSpawnerBuildingPrefab
                : spawnManager.MecaBaseSpawnerBuildingPrefab;

            var basicUnitPrefab = playerSpecies == SpeciesType.Slime
                ? spawnManager.SlimeBasicUnitPrefab
                : spawnManager.MecaBasicUnitPrefab;

            var numberOfBaseSpawner = i == 1
                ? spawnManager.NumberOfBaseSpawnerForPlayerOne
                : spawnManager.NumberOfBaseSpawnerForPlayerTwo;

            var numberOfUnitPerBaseSpawner = playerSpecies == SpeciesType.Slime
                ? spawnManager.NumberOfSlimeUnitPerSlimeBaseSpawner
                : spawnManager.NumberOfMecaUnitPerMecaBaseSpawner;

            var startPosition = i == 1
                ? spawnManager.StartPositionBaseSpawnerPlayerOne
                : spawnManager.StartPositionBaseSpawnerPlayerTwo;

            var playerHand = state.EntityManager.Instantiate(playerHandPrefab);

            state.EntityManager.SetComponentData(playerHand, new LocalTransform
            {
                Position = startPosition,
                Scale = 1,
                Rotation = quaternion.identity
            });

            state.EntityManager.SetComponentData(playerHand, new Player
            {
                PlayerNumber = i,
                PlayerSpecies = playerSpecies,
                NbOfBaseSpawnerBuilding = numberOfBaseSpawner,
                NbOfUnitPerBaseSpawnerBuilding = numberOfUnitPerBaseSpawner,
                StartPosition = startPosition,
                Winner = false,

                BaseSpawnerBuildingPrefab = baseSpawnerBuildingPrefab,
                BasicUnitPrefab = basicUnitPrefab
            });
        }

        Debug.Log("Players successfully created!");
    }
}