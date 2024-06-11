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
public partial struct UpgradedUnitSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Game>();
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<SpawnManager>();
        state.RequireForUpdate<BaseSpawnerBuilding>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();

        if (!configManager.ActivateMecaBasicUnitUpgradeSystem)
        {
            state.Enabled = false;
            return;
        }

        if (configManager.IsGamePaused)
            return;
        
        var spawnManager = SystemAPI.GetSingleton<SpawnManager>();

        if (spawnManager.SpawnUnitWhenPressEnter)
        {
            if (!Input.GetKeyDown(KeyCode.Return))
                return;
            Debug.Log("Enter detected! Spawning unit now!");
        }

        var ecbJob = new EntityCommandBuffer(Allocator.TempJob);

        foreach (var (transform, spawner, upgrades) 
                 in SystemAPI.Query<RefRO<LocalTransform>, RefRW<BaseSpawnerBuilding>, RefRO<SpawnerUpgradesRegister>>())
        {
            if (spawner.ValueRO.TimeToNextGeneration > 0f)
            {
                spawner.ValueRW.TimeToNextGeneration -= SystemAPI.Time.DeltaTime;
                continue;
            }
            spawner.ValueRW.TimeToNextGeneration = spawner.ValueRO.GenerationInterval;

            var unitSpawnJob = new UpgradedUnitSpawnJob
            {
                CommandBuffer = ecbJob.AsParallelWriter(),
                Prefab = spawner.ValueRO.SpawnedUnitPrefab,
                UnitPosition = spawner.ValueRO.UnitInitialPosition,
                UnitRotation = spawner.ValueRO.UnitInitialRotation,
                UnitScale = spawner.ValueRO.UnitInitialScale,
                BasePosition = transform.ValueRO.Position, // Spawn a unit, position it at near the base spawner player's location
                TotalUnits = spawner.ValueRO.NbOfUnitPerBase,
                UnitSpace = 2f, // NOTE: Default space to 2f for x and y axis
                GroupUnitsBy = GroupUnitShape.Line,
                UpgradesRegister = upgrades.ValueRO
            };
            var unitSpawnJobHandler = unitSpawnJob.Schedule((int)spawner.ValueRO.NbOfUnitPerBase, 64, state.Dependency);
            state.Dependency = unitSpawnJobHandler;

            unitSpawnJobHandler.Complete();
        }

        ecbJob.Playback(state.EntityManager);
        ecbJob.Dispose();

        // If this was the initial spawn wave, start the game
        var singleton = SystemAPI.GetSingleton<Game>();
        if (singleton.State == GameState.Starting)
        {
            singleton.State = GameState.Running;
            SystemAPI.SetSingleton(singleton);
        }
    }
}

[BurstCompile]
public struct UpgradedUnitSpawnJob : IJobParallelFor
{
    public EntityCommandBuffer.ParallelWriter CommandBuffer;
    public Entity Prefab;
    public float3 UnitPosition;
    public Quaternion UnitRotation;
    public float UnitScale;
    public float3 BasePosition;
    public uint TotalUnits;
    public float UnitSpace;
    public GroupUnitShape GroupUnitsBy;
    public SpawnerUpgradesRegister UpgradesRegister;

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
            Rotation = UnitRotation,
            Scale = UnitScale
        });
        ApplyUpgrades(index, instance);
    }

    private void ApplyUpgrades(int index, Entity entity)
    {
        if (UpgradesRegister.HasGlassCannon)
            CommandBuffer.AddComponent<GlassCannonUpgrade>(index, entity);
        
        if (UpgradesRegister.HasArtillery)
            CommandBuffer.AddComponent<ArtilleryUpgrade>(index, entity);

        if (UpgradesRegister.HasGatling)
            CommandBuffer.AddComponent<GatlingUpgrade>(index, entity);
        
        if (UpgradesRegister.HasScout)
            CommandBuffer.AddComponent<ScoutUpgrade>(index, entity);
    }
}