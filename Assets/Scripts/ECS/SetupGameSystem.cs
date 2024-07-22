using AnimCooker;
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
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<SoundManager>();
        state.RequireForUpdate<SpawnManager>();
        state.RequireForUpdate<Game>();
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<SimpleLodOptsData>();
    }

    // [BurstCompile] using GameObject.Find
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();
        var gameManager = SystemAPI.GetSingleton<Game>();
        var soundManager = SystemAPI.GetSingleton<SoundManager>();

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
            var graphicQualityLevel = GraphicQuality.Default;
            var language = Language.English;
            var soundVolume = 1f;
            var soundBackgroundVolume = 0.3f;

            var gameManagerGameObject = GameObject.Find("GameManager");

            if (gameManagerGameObject != null)
            {
                var gameManagerFromMonobehaviour = gameManagerGameObject.GetComponent<GameManager>();
                if (gameManagerFromMonobehaviour != null)
                {
                    difficulty = gameManagerFromMonobehaviour.GetDifficulty();
                    speciesToPlay = gameManagerFromMonobehaviour.GetSpeciesToPlay();
                    playerName = gameManagerFromMonobehaviour.GetPlayerNameAsFixedString();
                    graphicQualityLevel = gameManagerFromMonobehaviour.GetGraphicQualityLevel();
                    language = gameManagerFromMonobehaviour.GetLanguage();
                }

                var soundManagerFromMonobehaviour = gameManagerGameObject.GetComponent<SoundManagerMono>();
                if (soundManagerFromMonobehaviour != null)
                {
                    soundVolume = soundManagerFromMonobehaviour.Volume;
                    soundBackgroundVolume = soundManagerFromMonobehaviour.BackgroundVolume;
                    // NOTE: Switch to battlefield theme sound when launching game
                    soundManagerFromMonobehaviour.PlayBackgroundMusic("UIBattlefield");
                }
            }

            Debug.Log($"Player Name: {playerName}");
            Debug.Log($"Difficulty: {difficulty}");
            Debug.Log($"Species To Play: {speciesToPlay}");
            Debug.Log($"Graphic Quality Level: {graphicQualityLevel}");
            Debug.Log($"Language: {language}");
            Debug.Log($"Sound Volume: {soundVolume}");
            Debug.Log($"Sound Background Volume: {soundBackgroundVolume}");

            Debug.Log("Starting now");

            var spawnManager = SystemAPI.GetSingleton<SpawnManager>();

            // NOTE: If we allow more than 2 players, adjust the loop here
            for (uint i = 1; i <= 2; i++)
            {
                // TODO: Instantiate the hand entity for each player (the hand will replace the cursor of the mouse for more immersion).
                var playerSpecies = i == 1
                    ? SpeciesType.Slime
                    : SpeciesType.Meca;

                var playerHandPrefab = playerSpecies == SpeciesType.Slime
                    ? spawnManager.SlimePlayerHandPrefab
                    : spawnManager.MecaPlayerHandPrefab;

                var numberOfBaseSpawnerAtStart = i == 1
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
                    StartNbOfBaseSpawnerBuilding = numberOfBaseSpawnerAtStart,
                    StartPosition = startPosition
                });
            }

            Debug.Log("Players successfully created!");

            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (playerInfos, species) in
                     SystemAPI.Query<RefRO<Player>, RefRO<SpeciesTag>>()
                         .WithAll<Player>())
            {
                var nbBaseSpawner = playerInfos.ValueRO.StartNbOfBaseSpawnerBuilding;
                var isBuildingControlledByAI = GameManager.IsControlledByAI(gameManager.SpeciesToPlay, species.ValueRO.Type);

                // if (isBuildingControlledByAI)
                // {
                //     switch (difficulty)
                //     {
                //         case Difficulty.Easy:
                //             break;
                //         case Difficulty.Medium:
                //             nbBaseSpawner += 1;
                //             break;
                //         case Difficulty.Hard:
                //             nbBaseSpawner += 2;
                //             break;
                //         case Difficulty.Nightmare:
                //             nbBaseSpawner += 3;
                //             break;
                //     }
                // }
                if (isBuildingControlledByAI && difficulty == Difficulty.Nightmare)
                    nbBaseSpawner += 2;

                var offsetMultiplier = 2f; // Multiplier for the offset based on the scale

                Debug.Log($"nbBaseSpawner: {nbBaseSpawner}");

                // TODO & FIXME: Continue here
                var baseSpawnerSlimeCount = 0;

                for (var i = 0; i < nbBaseSpawner; i++)
                {
                    var baseSpawnerBuildingPrefab = species.ValueRO.Type == SpeciesType.Slime ? baseSpawnerSlimeCount == 0 ? spawnManager.SlimeBasicWaterUnitBaseSpawnerBuildingPrefab : baseSpawnerSlimeCount == 1 ? spawnManager.SlimeBasicFireUnitBaseSpawnerBuildingPrefab : spawnManager.SlimeBasicAirUnitBaseSpawnerBuildingPrefab : spawnManager.MecaBasicUnitBaseSpawnerBuildingPrefab;

                    if (species.ValueRO.Type == SpeciesType.Slime)
                    {
                        baseSpawnerSlimeCount++;
                    }

                    var ltBaseSpawner = state.EntityManager.GetComponentData<LocalTransform>(baseSpawnerBuildingPrefab);

                    float positionOffset;

                    if (i == 0)
                    {
                        positionOffset = 0f; // First spawner at the start position
                    }
                    else
                    {
                        // Calculate the offset for subsequent spawners
                        var offset = (i + 1) / 2 * offsetMultiplier * ltBaseSpawner.Scale;
                        var direction = i % 2 == 0 ? 1 : -1; // Alternate direction: right for even, left for odd
                        positionOffset = offset * direction;
                    }

                    var position = new float3(
                        playerInfos.ValueRO.StartPosition.x + positionOffset,
                        playerInfos.ValueRO.StartPosition.y,
                        playerInfos.ValueRO.StartPosition.z
                    );

                    var baseSpawnerPlayer = state.EntityManager.Instantiate(baseSpawnerBuildingPrefab);

                    state.EntityManager.SetComponentData(baseSpawnerPlayer, new LocalTransform
                    {
                        Position = position + ltBaseSpawner.Position, // NOTE: Add a offset position
                        Scale = ltBaseSpawner.Scale,
                        Rotation = ltBaseSpawner.Rotation
                    });

                    if (isBuildingControlledByAI)
                    {
                        ecbSingleton.AddComponent<AI>(baseSpawnerPlayer);
                    }
                }
            }

            Debug.Log("Players base unit spawners building successfully created!");

            // NOTE: Let the system choose the appropriate graphic quality if graphicQualityLevel is set to default.
            if (graphicQualityLevel != GraphicQuality.Default)
            {
                // Adjust LOD settings based on graphic quality
                var lodOpts = SystemAPI.GetSingleton<SimpleLodOptsData>();

                switch (graphicQualityLevel)
                {
                    case GraphicQuality.Low:
                        lodOpts.TimerInterval = 1f; // NOTE: LOD updates less frequently for low quality
                        lodOpts.ForceLodHeightLevel = ForceLodHeightLevel.TwoAliasPoorQuality;
                        break;
                    case GraphicQuality.Medium:
                        lodOpts.TimerInterval = 0.5f;
                        lodOpts.ForceLodHeightLevel = ForceLodHeightLevel.OneAliasMediumQuality;
                        break;
                    case GraphicQuality.High:
                        lodOpts.TimerInterval = 0.25f; // NOTE: LOD updates more frequently for high quality
                        lodOpts.ForceLodHeightLevel = ForceLodHeightLevel.None;
                        break;
                    case GraphicQuality.Ultra:
                        lodOpts.TimerInterval = 0.1f;
                        lodOpts.ForceLodHeightLevel = ForceLodHeightLevel.ZeroAliasBestQuality;
                        break;
                }

                SystemAPI.SetSingleton(lodOpts);

                Debug.Log("Setting graphic quality done.");
            }

            soundManager.Volume = soundVolume;
            soundManager.BackgroundVolume = soundBackgroundVolume;
            SystemAPI.SetSingleton(soundManager);

            // NOTE: Start the game
            gameManager.State = GameState.Running;
            gameManager.RessourceCount = 0;
            gameManager.PlayerName = playerName;
            gameManager.Difficulty = difficulty;
            gameManager.SpeciesToPlay = speciesToPlay;
            gameManager.GraphicQualityLevel = graphicQualityLevel;
            gameManager.Language = language;
            SystemAPI.SetSingleton(gameManager);
        }
    }
}