using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[UpdateBefore(typeof(TransformSystemGroup))]
[UpdateBefore(typeof(UnitSpawnerSystem))]
[UpdateBefore(typeof(UpgradedUnitSpawnerSystem))]
[UpdateBefore(typeof(PauseScreenSystem))]
public partial struct SetupGameSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnManager>();
        state.RequireForUpdate<Game>();
        state.RequireForUpdate<Config>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();
        var gameManager = SystemAPI.GetSingleton<Game>();

        if (!configManager.ActivateSetupGameSystem)
        {
            state.Enabled = false;
            return;
        }

        if (gameManager.State == GameState.Starting)
        {
            Debug.Log("Starting now");
            var spawnManager = SystemAPI.GetSingleton<SpawnManager>();

            // NOTE: If we allow more than 2 players, adjust the loop here
            for (uint i = 1; i <= 2; i++)
            {
                //  ODO: Instantiate the hand entity for each player (the hand will replace the cursor of the mouse for more immersion).
                var playerSpecies = i == 1
                    ? spawnManager.PlayerOneSpecies
                    : spawnManager.PlayerTwoSpecies;

                var playerHandPrefab = playerSpecies == SpeciesType.Slime
                    ? spawnManager.SlimePlayerHandPrefab
                    : spawnManager.MecaPlayerHandPrefab;

                var baseSpawnerBuildingPrefab = playerSpecies == SpeciesType.Slime
                    ? spawnManager.SlimeBaseSpawnerBuildingPrefab
                    : spawnManager.MecaBaseSpawnerBuildingPrefab;

                var numberOfBaseSpawner = i == 1
                    ? spawnManager.NumberOfBaseSpawnerForPlayerOne
                    : spawnManager.NumberOfBaseSpawnerForPlayerTwo;


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
                    StartPosition = startPosition,
                    BaseSpawnerBuildingPrefab = baseSpawnerBuildingPrefab
                });
            }

            Debug.Log("Players successfully created!");

            foreach (var playerInfos in
                     SystemAPI.Query<RefRO<Player>>()
                         .WithAll<Player>())
            {
                var rand = new Random(playerInfos.ValueRO.PlayerNumber);

                var offsetBaseSpawnerBuilding = playerInfos.ValueRO.NbOfBaseSpawnerBuilding == 1
                    ? 0
                    : rand.NextFloat(playerInfos.ValueRO.NbOfBaseSpawnerBuilding);

                var baseSpawnerPlayer =
                    state.EntityManager.Instantiate(playerInfos.ValueRO.BaseSpawnerBuildingPrefab);

                // Position the new base building spawner by setting its LocalTransform component.
                state.EntityManager.SetComponentData(baseSpawnerPlayer, new LocalTransform
                {
                    Position = new float3
                    {
                        x = playerInfos.ValueRO.StartPosition.x + offsetBaseSpawnerBuilding,
                        y = playerInfos.ValueRO.StartPosition.y,
                        z = playerInfos.ValueRO.StartPosition.z + offsetBaseSpawnerBuilding
                    },
                    Scale = 1,
                    Rotation = quaternion.identity
                });
            }

            Debug.Log("Players base unit spawners building successfully created!");

            // NOTE: Start the game
            gameManager.State = GameState.Running;
            gameManager.RessourceCount = 0;
            SystemAPI.SetSingleton(gameManager);
        }
    }
}