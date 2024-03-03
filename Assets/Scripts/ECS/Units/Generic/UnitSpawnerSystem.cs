using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

// This UpdateBefore is necessary to ensure the unit get rendered in
// the correct position for the frame in which they're spawned.
// If the unit spawning system differs significantly between units, we should implement a specialized system, such as MySlimeUnitSpawningSystem, instead of a generic one like this one.
[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct UnitSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<SpawnManager>();
        state.RequireForUpdate<BaseSpawnerBuilding>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();

        if (!configManager.ActivateUnitSpawnerSystem)
        {
            state.Enabled = false;
            return;
        }

        var spawnManager = SystemAPI.GetSingleton<SpawnManager>();

        if (spawnManager.SpawnUnitWhenPressEnter)
        {
            if (!Input.GetKeyDown(KeyCode.Return))
                return;
            Debug.Log("Enter detected! Spawning unit now!");
        }
        else
        {
            state.Enabled = false; // Let it spawn only one time if it does it automatically
        }

        var ecbJob = new EntityCommandBuffer(Allocator.TempJob);

        // TODO: In the future, when we're developing buildings that spawn units upon click or according to our chosen criteria, we'll need to refactor the code below.
        // TODO: See if we use a System base instead of the current implementation, with something like:
        // Entities
        // .WithDeferredPlaybackSystem<EndSimulationEntityCommandBufferSystem>()
        // .ForEach(
        //     (Entity entity, EntityCommandBuffer ecb, in BaseSpawnerBuilding baseSpawner) =>
        //     {
        //         // Spawn unit here with EntityCommandBuffer
        //     }
        // ).ScheduleParallel();
        for (uint i = 0; i < 2; i++)
        {
            var playerSpecies = i == 0
                ? spawnManager.PlayerOneSpecies
                : spawnManager.PlayerTwoSpecies;

            var spawnedUnitPrefab = playerSpecies == SpeciesType.Slime
                ? spawnManager.SlimeBasicUnitPrefab
                : spawnManager.MecaBasicUnitPrefab;

            var typeOfUnit = spawnedUnitPrefab == spawnManager.SlimeBasicUnitPrefab
                ? UnitType.SlimeBasic
                : UnitType.MecaBasic;

            var nbOfUnitPerBase = playerSpecies == SpeciesType.Slime
                ? spawnManager.NumberOfSlimeUnitPerSlimeBaseSpawner
                : spawnManager.NumberOfMecaUnitPerMecaBaseSpawner;

            var startPosition = i == 0
                ? spawnManager.StartPositionBaseSpawnerPlayerOne
                : spawnManager.StartPositionBaseSpawnerPlayerTwo;

            // Spawn most of the entities in a Burst job by cloning a pre-created prototype entity,
            // which can be either an entity created at run time or a Prefab like here.
            // This is the fastest and most efficient way to create entities at run time.
            var unitSpawnerJob = new UnitSpawnerJob
            {
                CommandBuffer = ecbJob.AsParallelWriter(),
                Species = playerSpecies,
                TypeOfUnit = typeOfUnit,
                Prefab = spawnedUnitPrefab,
                BasePosition = startPosition, // Spawn a unit, position it at near the base spawner player's location
                TotalUnits = nbOfUnitPerBase,
                UnitSpace = 2f, // NOTE: Default space to 1f for x and y axis
                GroupUnitsBy = spawnManager.GroupUnitsBy
            };

            var unitSpawnerJobHandle = unitSpawnerJob.Schedule((int)nbOfUnitPerBase, 64, state.Dependency);
            state.Dependency = unitSpawnerJobHandle;

            unitSpawnerJobHandle.Complete();
        }

        ecbJob.Playback(state.EntityManager);
        ecbJob.Dispose();
    }
}

public enum GroupUnitShape
{
    BlocsSquare,
    Square,
    Line
}

[BurstCompile]
public struct UnitSpawnerJob : IJobParallelFor
{
    public EntityCommandBuffer.ParallelWriter CommandBuffer;
    public SpeciesType Species;
    public UnitType TypeOfUnit;
    public Entity Prefab;
    public float3 BasePosition;
    public uint TotalUnits;
    public float UnitSpace;
    public GroupUnitShape GroupUnitsBy;

    public void Execute(int index)
    {
        int x, z;
        var position = float3.zero;

        if (GroupUnitsBy is GroupUnitShape.BlocsSquare)
        {
            // Create bloc squares units of 100 by 100 with 2 units of space between each group
            var blockSize = 100;
            var unitsPerRow = (int)math.sqrt(blockSize);
            var groupSize = (int)math.ceil((float)TotalUnits / blockSize);
            var groupCountPerRow = (int)math.ceil(math.sqrt(groupSize));
            var groupIndex = index / blockSize;
            var indexInGroup = index % blockSize;
            var groupX = groupIndex % groupCountPerRow;
            var groupZ = groupIndex / groupCountPerRow;

            x = indexInGroup % unitsPerRow + groupX * (unitsPerRow + 2); // 2 units of space between groups horizontally
            z = indexInGroup / unitsPerRow + groupZ * (unitsPerRow + 2); // 2 units of space between groups vertically

            // Calculate the offset to center the units
            var totalGroupsX = groupCountPerRow * (unitsPerRow + 2) - 2; // Subtract 2 to account for the extra space after the last group
            var totalGroupsZ = groupCountPerRow * (unitsPerRow + 2) - 2; // Same as above
            var offsetX = (totalGroupsX - 1) * UnitSpace * 0.5f;
            var offsetZ = (totalGroupsZ - 1) * UnitSpace * 0.5f;

            position = new float3
            {
                x = BasePosition.x + x * UnitSpace - offsetX,
                y = BasePosition.y,
                z = BasePosition.z + z * UnitSpace - offsetZ
            };
        }
        else if (GroupUnitsBy is GroupUnitShape.Square or GroupUnitShape.Line)
        {
            // NOTE: Else just arrange unit by columns and rows
            var cols = (int)math.ceil(math.sqrt(TotalUnits));
            var rows = (int)math.ceil((float)TotalUnits / cols);
            x = index % cols;
            z = index / cols;

            if (GroupUnitsBy is GroupUnitShape.Line)
            {
                // Ensure we don't exceed the maximum number of lines if we choose group by line
                var maxLines = 10;
                cols = (int)math.ceil((float)TotalUnits / maxLines);
                rows = math.min(maxLines, (int)TotalUnits);
                x = index % cols;
                z = index / cols;

                z = math.min(z, maxLines - 1);
            }

            // Calculate the offset to center the units
            var offsetX = (cols - 1) * UnitSpace * 0.5f;
            var offsetZ = (rows - 1) * UnitSpace * 0.5f;

            // Apply the spacing
            position = new float3
            {
                x = BasePosition.x + x * UnitSpace - offsetX,
                y = BasePosition.y,
                z = BasePosition.z + z * UnitSpace - offsetZ
            };
        }

        var instance = CommandBuffer.Instantiate(index, Prefab);
        CommandBuffer.SetComponent(index, instance, new LocalTransform
        {
            Position = position,
            Rotation = quaternion.identity,
            Scale = 1
        });

        if (Species == SpeciesType.Slime && TypeOfUnit == UnitType.SlimeBasic)
        {
            CommandBuffer.AddComponent(index, instance, new SlimeBasicUnitMerge
            {
                NbUnitsToMerge = 10,
                MergedUnitType = UnitType.SlimeStronger
            });
        }
    }
}