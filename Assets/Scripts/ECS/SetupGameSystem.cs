using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(TransformSystemGroup))]
[UpdateBefore(typeof(UnitSpawnerSystem))]
[UpdateBefore(typeof(UpgradedUnitSpawnerSystem))]
[UpdateBefore(typeof(SelectableSystem))]
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

    // [BurstCompile] using GameObject.Find
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
            var difficulty = Difficulty.Medium;
            var speciesToPlay = SpeciesToPlay.Both; // NOTE: Both by default for easy testing
            var playerName = new FixedString32Bytes("Mika");

            var gameManagerGameObject = GameObject.Find("GameManager");

            if (gameManagerGameObject != null)
            {
                var gameManagerFromMonobehaviour = gameManagerGameObject.GetComponent<GameManager>();
                difficulty = gameManagerFromMonobehaviour.GetDifficulty();
                speciesToPlay = gameManagerFromMonobehaviour.GetSpeciesToPlay();
                playerName = gameManagerFromMonobehaviour.GetPlayerNameAsFixedString();
            }

            Debug.Log($"Player Name: {playerName}");
            Debug.Log($"Difficulty: {difficulty}");
            Debug.Log($"Species To Play: {speciesToPlay}");

            Debug.Log("Starting now");

            var spawnManager = SystemAPI.GetSingleton<SpawnManager>();

            // NOTE: If we allow more than 2 players, adjust the loop here
            for (uint i = 1; i <= 2; i++)
            {
                // TODO: Instantiate the hand entity for each player (the hand will replace the cursor of the mouse for more immersion).
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
                    ? spawnManager.NumberOfStartingBaseSpawnerForSlime
                    : spawnManager.NumberOfStartingBaseSpawnerForMeca;


                var startPosition = i == 1
                    ? spawnManager.StartPositionBaseSpawnerSlime
                    : spawnManager.StartPositionBaseSpawnerMeca;

                // NOTE: Initial placement based on the selected player species
                var zPlacement = speciesToPlay == SpeciesToPlay.Meca ? i == 1 ? 14 : -14 : i == 1 ? -14 : 14;
                startPosition.z = zPlacement;

                var playerHand = state.EntityManager.Instantiate(playerHandPrefab);

                state.EntityManager.SetComponentData(playerHand, new LocalTransform
                {
                    Position = startPosition + new float3(0, -500, 0), // NOTE: Hide player hand for the moment
                    Scale = 1,
                    Rotation = quaternion.identity
                });

                state.EntityManager.SetComponentData(playerHand, new Player
                {
                    PlayerNumber = i,
                    NbOfBaseSpawnerBuilding = numberOfBaseSpawner,
                    StartPosition = startPosition,
                    BaseSpawnerBuildingPrefab = baseSpawnerBuildingPrefab
                });
            }

            Debug.Log("Players successfully created!");

            foreach (var (playerInfos, species) in
                     SystemAPI.Query<RefRO<Player>, RefRO<SpeciesTag>>()
                         .WithAll<Player>())
            {
                var nbBaseSpawner = playerInfos.ValueRO.NbOfBaseSpawnerBuilding;
                var isBuildingControlledByAI = GameManager.IsControlledByAI(gameManager.SpeciesToPlay, species.ValueRO.Type);
                if (isBuildingControlledByAI)
                {
                    switch (difficulty)
                    {
                        case Difficulty.Easy:
                            break;
                        case Difficulty.Medium:
                            nbBaseSpawner += 1;
                            break;
                        case Difficulty.Hard:
                            nbBaseSpawner += 2;
                            break;
                    }
                }

                var scaleBaseSpawner = state.EntityManager.GetComponentData<LocalTransform>(playerInfos.ValueRO.BaseSpawnerBuildingPrefab).Scale;
                var offsetMultiplier = 1.5f; // Multiplier for the offset based on the scale

                Debug.Log($"nbBaseSpawner: {nbBaseSpawner}");
                for (var i = 0; i < nbBaseSpawner; i++)
                {
                    Debug.Log("Dans boucle");
                    float positionOffset;

                    if (i == 0)
                    {
                        positionOffset = 0f; // First spawner at the start position
                    }
                    else
                    {
                        // Calculate the offset for subsequent spawners
                        var offset = (i + 1) / 2 * offsetMultiplier * scaleBaseSpawner;
                        var direction = i % 2 == 0 ? 1 : -1; // Alternate direction: right for even, left for odd
                        positionOffset = offset * direction;
                    }

                    var position = new float3(
                        playerInfos.ValueRO.StartPosition.x + positionOffset,
                        playerInfos.ValueRO.StartPosition.y,
                        playerInfos.ValueRO.StartPosition.z
                    );

                    var baseSpawnerPlayer = state.EntityManager.Instantiate(playerInfos.ValueRO.BaseSpawnerBuildingPrefab);

                    state.EntityManager.SetComponentData(baseSpawnerPlayer, new LocalTransform
                    {
                        Position = position,
                        Scale = scaleBaseSpawner,
                        Rotation = quaternion.identity
                    });
                }
            }

            Debug.Log("Players base unit spawners building successfully created!");

            // NOTE: Start the game
            gameManager.State = GameState.Running;
            gameManager.RessourceCount = 0;
            gameManager.PlayerName = playerName;
            gameManager.Difficulty = difficulty;
            gameManager.SpeciesToPlay = speciesToPlay;
            SystemAPI.SetSingleton(gameManager);
        }
    }
}