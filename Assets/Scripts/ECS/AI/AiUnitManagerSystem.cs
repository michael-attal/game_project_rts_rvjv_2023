using AnimCooker;
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
public partial struct AiUnitManagerSystem : ISystem
{
    private int nbOfUnitsToAssignToGatherRessource; // NOTE: Number of units assigned to resource gathering

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Game>();
        state.RequireForUpdate<Config>();

        nbOfUnitsToAssignToGatherRessource = 10;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();
        var gameManager = SystemAPI.GetSingleton<Game>();

        if (!configManager.ActivateAiManagerSystem)
        {
            state.Enabled = false;
            return;
        }

        if (gameManager.State == GameState.Paused)
            return;

        if (!GameManager.IsAiPlaying(gameManager.SpeciesToPlay))
            return;

        var playerSpecies = gameManager.SpeciesToPlay;

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var resourceSeekers = CountRessourceSeeker(ref ecb, ref state, playerSpecies);

        // NOTE: Getting AI-controlled units
        // TODO: Consider developing an AI component and iterating over it in the future for improved performance.
        // TODO: Create an IsAttackingTag IEnable component to avoid changing every time the destination is reached with a new destination
        foreach (var (transform, unit, unitTypeTag, speciesTag, entity) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<Unit>, RefRO<UnitTypeTag>, RefRO<SpeciesTag>>().WithAll<UnitTypeTag>().WithNone<WantsToMove, WantsToGatherRessource, GatheringIntent>().WithEntityAccess())
        {
            if (GameManager.IsControlledByAI(playerSpecies, speciesTag.ValueRO.Type))
            {
                HandleUnitAssignment(ref ecb, ref state, entity, unitTypeTag, speciesTag, transform, playerSpecies, resourceSeekers);
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private int CountRessourceSeeker(ref EntityCommandBuffer ecb, ref SystemState state, SpeciesToPlay playerSpecies)
    {
        var resourceSeekers = 0;
        foreach (var (unitRessourceSeeker, species) in SystemAPI.Query<RefRO<GatheringIntent>, RefRO<SpeciesTag>>())
        {
            if (GameManager.IsControlledByAI(playerSpecies, species.ValueRO.Type))
            {
                resourceSeekers++;
            }
        }

        return resourceSeekers;
    }

    private void HandleUnitAssignment(ref EntityCommandBuffer ecb, ref SystemState state, Entity entity, RefRO<UnitTypeTag> unitTypeTag, RefRO<SpeciesTag> speciesTag, RefRO<LocalTransform> transform, SpeciesToPlay playerSpecies, int resourceSeekers)
    {
        // NOTE: Separation of units into resource units and attack units
        if (resourceSeekers < nbOfUnitsToAssignToGatherRessource)
        {
            // Always allocate the first 10 units to collecting resources.
            // TODO: Allocate 10 units per building
            ecb.SetComponentEnabled<WantsToGatherRessource>(entity, true);
        }
        else
        {
            HandleAttackUnits(ref ecb, ref state, entity, unitTypeTag, speciesTag, transform, playerSpecies);
        }
    }

    private void HandleAttackUnits(ref EntityCommandBuffer ecb, ref SystemState state, Entity entity, RefRO<UnitTypeTag> unitTypeTag, RefRO<SpeciesTag> speciesTag, RefRO<LocalTransform> transform, SpeciesToPlay playerSpecies)
    {
        // NOTE: 50% of units are merged if the AI plays the slime game
        if (Random.value > 0.5f && speciesTag.ValueRO.Type == SpeciesType.Slime && unitTypeTag.ValueRO.Type == UnitType.SlimeBasicWaterUnit)
        {
            ecb.SetComponentEnabled<WantsToMerge>(entity, true);
        }

        // NOTE: Now set attack logic for other units (80/20)
        var nearestEnemyUnitPos = GetNearestEnemyPosition(ref state, transform, true, playerSpecies);

        if (Random.value >= 0.2f && !nearestEnemyUnitPos.AreFloat3Equal(new float3(float.MaxValue, float.MaxValue, float.MaxValue)))
        {
            // NOTE: 80% of units attack the nearest enemy unit
            SetAttackMove(ref ecb, ref state, entity, nearestEnemyUnitPos);
        }
        else
        {
            // NOTE: 20% of units attack the nearest building
            var nearestEnemyBuildingPos = GetNearestEnemyPosition(ref state, transform, false, playerSpecies);
            SetAttackMove(ref ecb, ref state, entity, nearestEnemyBuildingPos);
        }
    }

    private float3 GetNearestEnemyPosition(ref SystemState state, RefRO<LocalTransform> transform, bool isUnit, SpeciesToPlay playerSpecies)
    {
        var nearestEnemyPos = new float3(float.MaxValue, float.MaxValue, float.MaxValue);

        if (isUnit)
        {
            foreach (var (unitLt, species) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<SpeciesTag>>().WithAll<Unit>())
            {
                if (GameManager.IsControlledByCurrentPlayer(playerSpecies, species.ValueRO.Type))
                {
                    if (transform.ValueRO.Position.DistanceTo(unitLt.ValueRO.Position) < transform.ValueRO.Position.DistanceTo(nearestEnemyPos))
                    {
                        nearestEnemyPos = unitLt.ValueRO.Position;
                    }
                }
            }
        }
        else
        {
            foreach (var (buildingLt, species) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<SpeciesTag>>().WithAll<BaseSpawnerBuilding>())
            {
                if (GameManager.IsControlledByCurrentPlayer(playerSpecies, species.ValueRO.Type))
                {
                    if (transform.ValueRO.Position.DistanceTo(buildingLt.ValueRO.Position) < transform.ValueRO.Position.DistanceTo(nearestEnemyPos))
                    {
                        nearestEnemyPos = buildingLt.ValueRO.Position;
                    }
                }
            }
        }

        return nearestEnemyPos;
    }

    private void SetAttackMove(ref EntityCommandBuffer ecb, ref SystemState state, Entity entity, float3 destination)
    {
        ecb.SetComponent(entity, new WantsToMove { Destination = destination });
        ecb.SetComponentEnabled<WantsToMove>(entity, true);

        // TODO: Move it inside the MoveOrder system instead of adding it here (same for GatheringSystem)
        if (SystemAPI.HasComponent<Unit>(entity) && SystemAPI.HasComponent<AnimationCmdData>(entity))
        {
            ecb.SetComponent(entity, new AnimationCmdData
            {
                Cmd = AnimationCmd.SetPlayForever, ClipIndex = (short)AnimationsType.Move
            });
            ecb.SetComponent(entity, new AnimationSpeedData
            {
                PlaySpeed = SystemAPI.GetComponent<Unit>(entity).UnitSpeed
            });
        }
    }
}