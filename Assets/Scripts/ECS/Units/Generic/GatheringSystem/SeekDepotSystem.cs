using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(GatherRessourceSystem))]
partial struct SeekDepotSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        
        foreach (var (transform, entity) in
                 SystemAPI.Query<RefRO<LocalTransform>>()
                     .WithAll<HasRessource, GatheringIntent>()
                     .WithNone<WantsToMove, DestinationReached>()
                     .WithEntityAccess())
        {
            float minDistance = float.MaxValue;
            float3? minLocation = null;
            foreach (var (depositTransform, localToWorld) in 
                     SystemAPI.Query<RefRO<LocalTransform>, RefRO<LocalToWorld>>()
                         .WithAll<DepositPoint>())
            {
                var depositPosition = localToWorld.ValueRO.Value.TransformPoint(depositTransform.ValueRO.Position);
                var distance = depositPosition.DistanceTo(transform.ValueRO.Position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    minLocation = depositPosition;
                }
            }

            if (minLocation.HasValue)
            {
                ecb.SetComponent(entity, new WantsToMove()
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
