using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct SlimeBasicUnitMergeSystem : ISystem
{
    private EntityQuery query;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ParticleManager>();
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<Game>();
        state.RequireForUpdate<SpawnManager>();
        state.RequireForUpdate<UnitSelectable>();
        state.RequireForUpdate<SlimeBasicUnitMerge>();

        query = state.GetEntityQuery(typeof(SlimeBasicUnitMerge), typeof(UnitSelected), typeof(LocalToWorld));
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();

        if (!configManager.ActivateSlimeBasicUnitMergeSystem)
        {
            state.Enabled = false;
            return;
        }

        if (configManager.IsGamePaused)
            return;

        if (!Input.GetKeyDown(KeyCode.F))
            return;

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        var spawnManager = SystemAPI.GetSingleton<SpawnManager>();
        var slimeStrongerUnitPrefab = spawnManager.SlimeStrongerUnitPrefab;

        var entities = query.ToEntityArray(Allocator.TempJob);
        var positions = query.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);

        if (entities.Length < 10)
        {
            entities.Dispose();
            positions.Dispose();
            return;
        }

        var groupCount = entities.Length / 10;
        var defaultScale = state.EntityManager.GetComponentData<LocalTransform>(slimeStrongerUnitPrefab).Scale;

        var gameInfo = SystemAPI.GetSingleton<Game>();
        var particleManager = SystemAPI.GetSingleton<ParticleManager>();

        ref var slimeRecipes = ref gameInfo.SlimeRecipes.Value.Data;

        var fusionInfo = new FusionInfo();
        foreach (var entity in entities)
        {
            var slimeBasicUnitMerge = state.EntityManager.GetComponentData<SlimeBasicUnitMerge>(entity);
            fusionInfo += slimeBasicUnitMerge.FusionInfo;
        }

        var mergeUnitsJob = new MergeUnitsJob
        {
            ECB = ecb,
            MergedUnitPrefab = slimeStrongerUnitPrefab,
            DefaultScale = defaultScale,
            Entities = entities,
            Positions = positions,
            GroupCount = groupCount,
            FusionInfo = fusionInfo, // NOTE: Calculated according to the units selected
            SlimeRecipes = new NativeArray<FusionRecipeData>(slimeRecipes.Length, Allocator.TempJob),
            ParticleGeneratorPrefab = particleManager.ParticleGeneratorPrefab
        };

        // NOTE: Copy recipe data to NativeArray
        for (var i = 0; i < slimeRecipes.Length; i++)
        {
            mergeUnitsJob.SlimeRecipes[i] = slimeRecipes[i];
        }

        var handle = mergeUnitsJob.Schedule(groupCount, 1, state.Dependency);
        state.Dependency = handle;

        // NOTE: Command Buffer final for playback
        var finalEcb = new EntityCommandBuffer(Allocator.TempJob);

        state.Dependency = JobHandle.CombineDependencies(handle, state.Dependency);
        state.Dependency.Complete();

        finalEcb.Playback(state.EntityManager);
        finalEcb.Dispose();

        mergeUnitsJob.SlimeRecipes.Dispose();
    }
}

[BurstCompile]
public struct MergeUnitsJob : IJobParallelFor
{
    public EntityCommandBuffer.ParallelWriter ECB;
    public Entity MergedUnitPrefab;
    public float DefaultScale;
    [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> Entities;
    [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<LocalToWorld> Positions;
    public int GroupCount;
    public FusionInfo FusionInfo;
    [ReadOnly] public NativeArray<FusionRecipeData> SlimeRecipes;
    public Entity ParticleGeneratorPrefab;

    public void Execute(int index)
    {
        var averagePosition = float3.zero;
        for (var i = 0; i < 10; i++)
        {
            averagePosition += Positions[index * 10 + i].Position;
        }

        averagePosition /= 10;

        var newEntity = ECB.Instantiate(index, MergedUnitPrefab);

        ECB.SetComponent(index, newEntity, new LocalTransform
        {
            Position = averagePosition,
            Rotation = quaternion.identity,
            Scale = DefaultScale
        });
        ECB.SetComponentEnabled<UnitSelected>(index, newEntity, true);

        for (var i = 0; i < 10; i++)
        {
            ECB.DestroyEntity(index, Entities[index * 10 + i]);
        }

        for (var i = 0; i < SlimeRecipes.Length; i++)
        {
            if (SlimeRecipes[i].Cost <= FusionInfo)
            {
                FusionInfo -= SlimeRecipes[i].Cost;

                var particleGenerator = ECB.Instantiate(index, ParticleGeneratorPrefab);
                ECB.SetComponent(index, particleGenerator, new ParticleGeneratorData
                {
                    Rate = 50f,
                    LifetimeOfGenerator = 0.5f,
                    LifetimeOfParticle = 0.5f,
                    Size = DefaultScale,
                    Speed = 2f,
                    Direction = new float3(0, 1, 0),
                    Color = new float4(0, 0, 1, 0.5f),
                    IsRandomPositionParticleSpawningActive = true,
                    PositionRangeForRandomParticleSpawning = new float3(DefaultScale * 3, DefaultScale * 3, DefaultScale * 3)
                });
                ECB.SetComponent(index, particleGenerator, new LocalTransform
                {
                    Position = averagePosition,
                    Rotation = quaternion.identity,
                    Scale = 1f
                });
            }
        }
    }
}