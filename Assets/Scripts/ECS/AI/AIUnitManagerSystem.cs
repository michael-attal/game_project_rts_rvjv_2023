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

// NOTE: This system manages AI units, assigning them to gather resources or attack based on the game's state.
[UpdateInGroup(typeof(AISystemGroup))]
[UpdateAfter(typeof(SeekRessourceSystem))]
public partial struct AIUnitManagerSystem : ISystem
{
    private int nbOfUnitsToAssignToGatherRessource; // NOTE: Number of units assigned to resource gathering
    private int countAIUnits;
    private int maximumNbOfUnitsActive;
    private int nbOfUnitsActive;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Game>();
        state.RequireForUpdate<Config>();

        nbOfUnitsToAssignToGatherRessource = 10;
        countAIUnits = 0;
        maximumNbOfUnitsActive = 0;
        nbOfUnitsActive = 0;
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

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var resourceSeekers = CountRessourceSeeker(ref ecb, ref state);

        // Count AI units for the active unit limitation (depending on difficulty)
        countAIUnits = SystemAPI.QueryBuilder().WithAll<UnitTypeTag, AI>().Build().CalculateEntityCount();

        switch (gameManager.Difficulty)
        {
            case Difficulty.Easy:
                maximumNbOfUnitsActive = (int)(countAIUnits * 0.5f); // NOTE: Allow 50% of units to be active (attacking)
                break;
            case Difficulty.Medium:
                maximumNbOfUnitsActive = (int)(countAIUnits * 0.2f); // 20%
                break;
            case Difficulty.Hard:
                maximumNbOfUnitsActive = countAIUnits; // Full
                break;
            case Difficulty.Nightmare:
                maximumNbOfUnitsActive = countAIUnits;
                break;
        }

        // Count actual active units
        nbOfUnitsActive = SystemAPI.QueryBuilder()
            .WithAll<UnitTypeTag, AI>()
            .WithAny<WantsToMove, WantsToGatherRessource, GatheringIntent, IsAttackingTag>()
            .Build()
            .CalculateEntityCount();

        // NOTE: Getting AI-controlled units
        foreach (var (transform, unit, unitTypeTag, speciesTag, entity) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<Unit>, RefRO<UnitTypeTag>, RefRO<SpeciesTag>>().WithAll<AI>().WithNone<WantsToMove, WantsToGatherRessource>().WithNone<GatheringIntent, IsAttackingTag>().WithEntityAccess())
        {
            HandleUnitAssignment(ref ecb, ref state, entity, unitTypeTag, speciesTag, transform, gameManager, resourceSeekers);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private int CountRessourceSeeker(ref EntityCommandBuffer ecb, ref SystemState state)
    {
        var resourceSeekers = 0;
        foreach (var (unitRessourceSeeker, species) in SystemAPI.Query<RefRO<GatheringIntent>, RefRO<SpeciesTag>>().WithAll<AI>())
        {
            resourceSeekers++;
        }

        return resourceSeekers;
    }

    private void HandleUnitAssignment(ref EntityCommandBuffer ecb, ref SystemState state, Entity entity, RefRO<UnitTypeTag> unitTypeTag, RefRO<SpeciesTag> speciesTag, RefRO<LocalTransform> transform, Game gameManager, int resourceSeekers)
    {
        // NOTE: Separation of units into resource units and attack units
        if (resourceSeekers < nbOfUnitsToAssignToGatherRessource)
        {
            // Always allocate the first XX units to collecting resources.
            // TODO: Allocate XX units per building
            ecb.SetComponentEnabled<WantsToGatherRessource>(entity, true);
        }
        else
        {
            HandleAttackUnits(ref ecb, ref state, entity, unitTypeTag, speciesTag, transform, gameManager);
        }
    }

    private void HandleAttackUnits(ref EntityCommandBuffer ecb, ref SystemState state, Entity entity, RefRO<UnitTypeTag> unitTypeTag, RefRO<SpeciesTag> speciesTag, RefRO<LocalTransform> transform, Game gameManager)
    {
        // NOTE: Now set attack logic for other units (80/20)
        var nearestEnemyUnitPos = GetNearestEnemyPosition(ref state, transform, true);

        if (nbOfUnitsActive < maximumNbOfUnitsActive)
        {
            if (Random.value >= 0.2f && !nearestEnemyUnitPos.AreFloat3Equal(new float3(float.MaxValue, float.MaxValue, float.MaxValue)))
            {
                // NOTE: 80% of units attack the nearest enemy unit
                SetAttackMove(ref ecb, ref state, entity, nearestEnemyUnitPos);
            }
            else
            {
                // NOTE: 20% of units attack the nearest building
                var nearestEnemyBuildingPos = GetNearestEnemyPosition(ref state, transform, false);
                SetAttackMove(ref ecb, ref state, entity, nearestEnemyBuildingPos);
            }

            nbOfUnitsActive++;
        }
    }

    private float3 GetNearestEnemyPosition(ref SystemState state, RefRO<LocalTransform> transform, bool isUnit)
    {
        var nearestEnemyPos = new float3(float.MaxValue, float.MaxValue, float.MaxValue);

        if (isUnit)
        {
            // NOTE: Maybe target the most powerful unit first ?
            foreach (var (unitLt, species) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<SpeciesTag>>().WithAll<Unit>().WithNone<AI>())
            {
                if (transform.ValueRO.Position.DistanceTo(unitLt.ValueRO.Position) < transform.ValueRO.Position.DistanceTo(nearestEnemyPos))
                {
                    nearestEnemyPos = unitLt.ValueRO.Position;
                }
            }
        }
        else
        {
            foreach (var (buildingLt, species) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<SpeciesTag>>().WithAll<BaseSpawnerBuilding>().WithNone<AI>())
            {
                if (transform.ValueRO.Position.DistanceTo(buildingLt.ValueRO.Position) < transform.ValueRO.Position.DistanceTo(nearestEnemyPos))
                {
                    nearestEnemyPos = buildingLt.ValueRO.Position;
                }
            }
        }

        return nearestEnemyPos;
    }

    private void SetAttackMove(ref EntityCommandBuffer ecb, ref SystemState state, Entity entity, float3 destination)
    {
        ecb.SetComponent(entity, new WantsToMove { Destination = destination });
        ecb.SetComponentEnabled<WantsToMove>(entity, true);
    }
}