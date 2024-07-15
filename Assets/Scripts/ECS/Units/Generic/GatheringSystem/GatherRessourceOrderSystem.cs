using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

internal partial struct GatherRessourceOrderSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<Game>();
        state.RequireForUpdate<WantsToGatherRessource>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();
        var gameManager = SystemAPI.GetSingleton<Game>();

        if (!configManager.ActivateGatheringSystem)
        {
            state.Enabled = false;
            return;
        }

        if (gameManager.State == GameState.Paused)
            return;

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (transform, entity) in
                 SystemAPI.Query<RefRO<LocalTransform>>()
                     .WithAll<WantsToGatherRessource>()
                     .WithEntityAccess())
        {
            ecb.SetComponentEnabled<WantsToGatherRessource>(entity, false); // NOTE: Order to gather resources is given below, we can safely disable it now.

            if (SystemAPI.HasComponent<GatheringIntent>(entity))
            {
                ecb.RemoveComponent<GatheringIntent>(entity);
            }
            else
            {
                var minDistance = float.MaxValue;
                Entity? minLocation = null;
                foreach (var (gatherableTransform, gatherableEntity) in
                         SystemAPI.Query<RefRO<LocalTransform>>()
                             .WithAll<GatherableSpot>()
                             .WithEntityAccess())
                {
                    var gatherablePosition = gatherableTransform.ValueRO.Position;
                    var distance = gatherablePosition.DistanceTo(transform.ValueRO.Position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        minLocation = gatherableEntity;
                    }
                }

                if (minLocation.HasValue)
                {
                    ecb.AddComponent(entity, new GatheringIntent
                    {
                        AssignedSpot = minLocation.Value
                    });

                    if (SystemAPI.IsComponentEnabled<WantsToMove>(entity))
                        ecb.SetComponentEnabled<WantsToMove>(entity, false);
                }
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

public struct GatheringIntent : IComponentData
{
    public Entity AssignedSpot;
}

public struct HasRessource : IComponentData
{
    public int CarriedRessources;
}