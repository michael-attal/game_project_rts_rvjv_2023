using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(GatherRessourceSystem))]
internal partial struct SeekDepotSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<Game>();
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

        foreach (var (transform, unitSpeciesTag, entity) in
                 SystemAPI.Query<RefRO<LocalTransform>, RefRO<SpeciesTag>>()
                     .WithAll<HasRessource, GatheringIntent>()
                     .WithNone<WantsToMove, DestinationReached>()
                     .WithEntityAccess())
        {
            var minDistance = float.MaxValue;
            float3? minLocation = null;
            foreach (var (depositTransform, localToWorld, depositSpeciesTag) in
                     SystemAPI.Query<RefRO<LocalTransform>, RefRO<LocalToWorld>, RefRO<SpeciesTag>>()
                         .WithAll<DepositPoint>())
            {
                // NOTE: Only seek ally deposit
                if (depositSpeciesTag.ValueRO.Type == unitSpeciesTag.ValueRO.Type)
                {
                    var depositPosition = localToWorld.ValueRO.Value.TransformPoint(depositTransform.ValueRO.Position);
                    var distance = depositPosition.DistanceTo(transform.ValueRO.Position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        minLocation = depositPosition;
                    }
                }
            }

            if (minLocation.HasValue)
            {
                ecb.SetComponent(entity, new WantsToMove
                {
                    Destination = minLocation.Value
                });
                ecb.SetComponentEnabled<WantsToMove>(entity, true);
                ecb.RemoveComponent<DestinationReached>(entity);
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}