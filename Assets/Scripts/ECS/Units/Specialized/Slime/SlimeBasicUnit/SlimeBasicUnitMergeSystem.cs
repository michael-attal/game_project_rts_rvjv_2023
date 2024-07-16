using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct SlimeBasicUnitMergeSystem : ISystem
{
    private EntityQuery query;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<FusionOrder>();
        state.RequireForUpdate<ParticleManager>();
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<Game>();
        state.RequireForUpdate<Selectable>();
        state.RequireForUpdate<SlimeBasicUnitMerge>();

        query = state.GetEntityQuery(typeof(SlimeBasicUnitMerge), typeof(WantsToMerge), typeof(LocalToWorld));
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();
        var gameManager = SystemAPI.GetSingleton<Game>();

        if (!configManager.ActivateSlimeBasicUnitMergeSystem)
        {
            state.Enabled = false;
            return;
        }

        if (gameManager.State == GameState.Paused)
            return;

        // ONLY CONTINUE IF FUSION IS ORDERED
        if (!SystemAPI.TryGetSingleton(out FusionOrder fusionOrder))
            return;


        if (fusionOrder.Amount > 1)
        {
            fusionOrder.Amount -= 1;
            SystemAPI.SetSingleton(fusionOrder);
        }
        else
        {
            state.EntityManager.RemoveComponent<FusionOrder>(SystemAPI.GetSingletonEntity<FusionOrder>());
        }

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        
        // GET FUSING SLIMES
        var entities = query.ToEntityArray(Allocator.Temp);

        var positions = query.ToComponentDataArray<LocalToWorld>(Allocator.Temp);
        var mergeInfos = query.ToComponentDataArray<SlimeBasicUnitMerge>(Allocator.Temp);

        var buffer = SystemAPI.GetBuffer<InstantiatableEntityData>(SystemAPI.GetSingletonEntity<Game>());
        var particleManager = SystemAPI.GetSingleton<ParticleManager>();

        // SORT SLIMES BY FUSION INFO
        for (int i = 0; i < mergeInfos.Length; ++i)
        {
            FusionInfo minInfo = mergeInfos[i].FusionInfo;
            int minIndex = i;
            for (int j = i; j < mergeInfos.Length; ++j)
            {
                if (mergeInfos[j].FusionInfo <= minInfo)
                {
                    minInfo = mergeInfos[j].FusionInfo;
                    minIndex = j;
                }
            }

            (entities[i], entities[minIndex]) = (entities[minIndex], entities[i]);
            (positions[i], positions[minIndex]) = (positions[minIndex], positions[i]);
            (mergeInfos[i], mergeInfos[minIndex]) = (mergeInfos[minIndex], mergeInfos[i]);
        }
        
        // SELECT REQUIRED SLIMES
        NativeList<Entity> selectedEntities = new NativeList<Entity>(Allocator.Temp);
        NativeList<LocalToWorld> selectedPositions = new NativeList<LocalToWorld>(Allocator.Temp);
        
        FusionInfo reachedCost = new FusionInfo();
        FusionInfo cost = fusionOrder.Data.Cost;
        int index = 0;
        while (index < entities.Length && reachedCost <= cost)
        {
            if (mergeInfos[index].FusionInfo <= cost)
            {
                selectedEntities.Add(entities[index]);
                selectedPositions.Add(positions[index]);
                reachedCost += mergeInfos[index].FusionInfo;
            }
            ++index;
        }

        entities.Dispose();
        positions.Dispose();
        mergeInfos.Dispose();
        
        // MERGE SELECTED SLIMES
        var mergeUnitsJob = new MergeUnitsJob
        {
            ECB = ecb,
            Entities = selectedEntities.ToArray(Allocator.TempJob),
            Positions = selectedPositions.ToArray(Allocator.TempJob),
            EntitiesCount = selectedEntities.Length,
            SlimeRecipe = fusionOrder.Data,
            ParticleGeneratorPrefab = particleManager.ParticleGeneratorPrefab,
            InstantiatableEntities = buffer.ToNativeArray(Allocator.TempJob)
        };

        var handle = mergeUnitsJob.Schedule(1, 1, state.Dependency);
        state.Dependency = handle;

        selectedEntities.Dispose();
        selectedPositions.Dispose();

        // Command Buffer final for playback
        var finalEcb = new EntityCommandBuffer(Allocator.TempJob);

        state.Dependency = JobHandle.CombineDependencies(handle, state.Dependency);
        state.Dependency.Complete();

        finalEcb.Playback(state.EntityManager);
        finalEcb.Dispose();

        mergeUnitsJob.InstantiatableEntities.Dispose();
    }
}

[BurstCompile]
public struct MergeUnitsJob : IJobParallelFor
{
    public EntityCommandBuffer.ParallelWriter ECB;
    [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> Entities;
    [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<LocalToWorld> Positions;
    public int EntitiesCount;
    [ReadOnly] public FusionRecipeData SlimeRecipe;
    public Entity ParticleGeneratorPrefab;
    [ReadOnly] public NativeArray<InstantiatableEntityData> InstantiatableEntities;

    public void Execute(int index)
    {
        var averagePosition = float3.zero;
        for (var i = 0; i < EntitiesCount; i++)
        {
            averagePosition += Positions[index * 10 + i].Position;
        }

        averagePosition /= EntitiesCount;

        for (var i = 0; i < EntitiesCount; i++)
        {
            ECB.DestroyEntity(index, Entities[i]);
        }
        
        InstantiateEntity(index, ECB, SlimeRecipe.PrefabId, averagePosition);
        GenerateParticles(index, averagePosition);
    }

    private void GenerateParticles(int index, float3 position)
    {
        var particleGenerator = ECB.Instantiate(index, ParticleGeneratorPrefab);
        ECB.SetComponent(index, particleGenerator, new ParticleGeneratorData
        {
            Rate = 50f,
            LifetimeOfGenerator = 0.5f,
            LifetimeOfParticle = 0.5f,
            Size = 1f,
            Speed = 2f,
            Direction = new float3(0, 1, 0),
            Color = new float4(0, 0, 1, 0.5f),
            IsRandomPositionParticleSpawningActive = true,
            PositionRangeForRandomParticleSpawning = new float3(1f * 3, 1f * 3, 1f * 3)
        });
        ECB.SetComponent(index, particleGenerator, new LocalTransform
        {
            Position = position,
            Rotation = quaternion.identity,
            Scale = 1f
        });
    }

    private void InstantiateEntity(int index, EntityCommandBuffer.ParallelWriter ecb, int id, float3 position)
    {
        for (var i = 0; i < InstantiatableEntities.Length; ++i)
        {
            if (InstantiatableEntities[i].EntityID == id)
            {
                var newEntity = ecb.Instantiate(index, InstantiatableEntities[i].Entity);
                ECB.SetComponent(index, newEntity, new LocalTransform
                {
                    Position = position,
                    Rotation = quaternion.identity,
                    Scale = 1f
                });
                return;
            }
        }
    }
}

public struct FusionOrder : IComponentData
{
    public FusionOrder(FusionRecipeData data)
    {
        Amount = 1;
        Data = data;
    }

    public FusionOrder(int amount, FusionRecipeData data)
    {
        Amount = amount;
        Data = data;
    }

    public int Amount;
    public FusionRecipeData Data;
}