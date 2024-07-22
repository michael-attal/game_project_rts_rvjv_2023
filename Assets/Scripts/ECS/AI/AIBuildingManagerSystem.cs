using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using EntityCommandBuffer = Unity.Entities.EntityCommandBuffer;
using ISystem = Unity.Entities.ISystem;
using Random = UnityEngine.Random;
using SystemAPI = Unity.Entities.SystemAPI;
using SystemState = Unity.Entities.SystemState;

// NOTE: This system manages AI building and upgrading logic.
[UpdateInGroup(typeof(AISystemGroup))]
public partial struct AIBuildingManagerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Game>();
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<SpawnManager>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();
        var gameManager = SystemAPI.GetSingleton<Game>();

        if (!configManager.ActivateAIManagerSystem)
        {
            state.Enabled = false;
            return;
        }

        if (gameManager.State == GameState.Paused)
            return;

        if (!GameManager.IsAiPlaying(gameManager.SpeciesToPlay))
            return;

        var aiSpecies = (SpeciesType)GameManager.GetAiSpecies(gameManager.SpeciesToPlay)!;

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        HandleBuildingAndUpgrade(ref ecb, ref state, gameManager, aiSpecies);

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private void HandleBuildingAndUpgrade(ref EntityCommandBuffer ecb, ref SystemState state, Game gameManager, SpeciesType aiSpecies)
    {
        var spawnManager = SystemAPI.GetSingleton<SpawnManager>();
        var mecaBasicBaseSpawnerCount = 0;

        foreach (var (transform, baseSpawner, speciesTag, entity) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<BaseSpawnerBuilding>, RefRO<SpeciesTag>>().WithAll<AI>().WithNone<UpgradedBaseSpawnerTag>().WithEntityAccess()) // NOTE: Can't put 4 components in the same WithNone ... Idk why
        {
            HandleBaseSpawnerBuildingAndUpgrading(ref ecb, ref state, gameManager, aiSpecies, transform, entity, ref mecaBasicBaseSpawnerCount, spawnManager);
        }

        // NOTE: If the game is not over, but there is only an upgraded base spawner, then first add a non-upgraded version in order to enter into the foreach loop.
        if (aiSpecies == SpeciesType.Meca && mecaBasicBaseSpawnerCount == 0 && gameManager.State == GameState.Running)
        {
            var mecaBaseSpawnerBuildingTransform = state.EntityManager.GetComponentData<LocalTransform>(spawnManager.MecaBasicUnitBaseSpawnerBuildingPrefab);
            CreateBaseSpawner(ref ecb, ref state, gameManager, spawnManager, aiSpecies, GetNewBuildingPosition(mecaBaseSpawnerBuildingTransform.Position, mecaBaseSpawnerBuildingTransform.Scale), mecaBaseSpawnerBuildingTransform.Scale);
        }
    }

    private void HandleBaseSpawnerBuildingAndUpgrading(ref EntityCommandBuffer ecb, ref SystemState state, Game gameManager, SpeciesType aiSpecies, RefRO<LocalTransform> transform, Entity entity, ref int mecaBasicBaseSpawnerCount, SpawnManager spawnManager)
    {
        if (aiSpecies == SpeciesType.Meca)
            mecaBasicBaseSpawnerCount++;

        // NOTE: Always build base spawner for Slime because there is no other cost
        if (Random.value > 0.5f || aiSpecies == SpeciesType.Slime)
        {
            // NOTE: Generate a building around another baseSpawner transform (randomly)
            var posNewBuilding = GetNewBuildingPosition(transform.ValueRO.Position, transform.ValueRO.Scale);
            CreateBaseSpawner(ref ecb, ref state, gameManager, spawnManager, aiSpecies, posNewBuilding, transform.ValueRO.Scale);
        }
        else
        {
            // NOTE: Always keep at least one basic spawner building
            // TODO: Update it if it consumes resources for upgrading the meca base spawner.
            if (mecaBasicBaseSpawnerCount > 1)
            {
                AddRandomUpgrade(ref ecb, entity);
            }
        }
    }

    private void CreateBaseSpawner(ref EntityCommandBuffer ecb, ref SystemState state, Game gameManager, SpawnManager spawnManager, SpeciesType aiSpecies, float3 position, float scale)
    {
        // Check if AI has enough resources to build a new base spawner
        if (gameManager.RessourceCountAI >= 50)
        {
            // Deduct the resource cost from AI's resources
            gameManager.RessourceCountAI -= 50;
            SystemAPI.SetSingleton(gameManager);

            // Instantiate the appropriate prefab based on the AI species
            var rdmValue = Random.value;
            var newBaseSpawner = ecb.Instantiate(aiSpecies == SpeciesType.Slime ? rdmValue > 0.75f ? spawnManager.SlimeBasicWaterUnitBaseSpawnerBuildingPrefab : rdmValue > 0.50f ? spawnManager.SlimeBasicFireUnitBaseSpawnerBuildingPrefab : spawnManager.SlimeBasicAirUnitBaseSpawnerBuildingPrefab : spawnManager.MecaBasicUnitBaseSpawnerBuildingPrefab);

            // Set the position, rotation, and scale for the new base spawner
            ecb.SetComponent(newBaseSpawner, new LocalTransform
            {
                Position = position,
                Rotation = quaternion.identity,
                Scale = scale
            });

            if (aiSpecies == SpeciesType.Meca)
            {
                ecb.AddComponent<SpawnerUpgradesRegister>(newBaseSpawner);
            }

            ecb.AddComponent<AI>(newBaseSpawner);
        }
    }

    private float3 GetNewBuildingPosition(float3 fromPosition, float fromScale)
    {
        var posNewBuilding = fromPosition;
        var direction = Random.value > 0.50f ? -1 : 1;
        posNewBuilding.x += Random.Range(fromScale * 2, fromScale * 4) * direction;
        posNewBuilding.z += Random.Range(fromScale * 2, fromScale * 4) * direction;

        return posNewBuilding;
    }

    private void AddRandomUpgrade(ref EntityCommandBuffer ecb, Entity entity)
    {
        var rdmValue = Random.value;

        if (rdmValue <= 0.25f)
        {
            ecb.AddComponent(entity, new SpawnerUpgradesRegister
            {
                HasGlassCannon = true
            });
        }
        else if (rdmValue <= 0.50f)
        {
            ecb.AddComponent(entity, new SpawnerUpgradesRegister
            {
                HasArtillery = true
            });
        }
        else if (rdmValue <= 0.75f)
        {
            ecb.AddComponent(entity, new SpawnerUpgradesRegister
            {
                HasGatling = true
            });
        }
        else
        {
            ecb.AddComponent(entity, new SpawnerUpgradesRegister
            {
                HasScout = true
            });
        }

        ecb.AddComponent(entity, new UpgradedBaseSpawnerTag());
    }
}