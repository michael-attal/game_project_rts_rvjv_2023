using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[UpdateAfter(typeof(UnitMovementSystem))]
partial struct SeekRessourceSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        
        foreach (var (gatherer, entity) in 
                 SystemAPI.Query<RefRO<GatheringIntent>>()
                     .WithNone<HasRessource, WantsToMove, DestinationReached>()
                     .WithEntityAccess())
        {
            var spotLocation = SystemAPI.GetComponent<LocalTransform>(gatherer.ValueRO.AssignedSpot).Position;
            ecb.AddComponent(entity, new WantsToMove()
            {
                Destination = spotLocation
            });
            ecb.SetComponentEnabled<WantsToMove>(entity, true);
        }
        
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
