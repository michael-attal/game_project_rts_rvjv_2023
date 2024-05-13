using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateAfter(typeof(SeekRessourceSystem))]
partial struct GatherRessourceSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GatheringIntent>();
        state.RequireForUpdate<UnitMovement>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);


        foreach (var (gatherer, entity) in
                 SystemAPI.Query<RefRO<GatheringIntent>>()
                     .WithAll<DestinationReached>()
                     .WithNone<HasRessource>()
                     .WithEntityAccess())
        {
            var ressourceCount = SystemAPI.GetComponent<GatherableSpot>(gatherer.ValueRO.AssignedSpot).RessourceAmount;
            ecb.AddComponent(entity, new HasRessource()
            {
                CarriedRessources = ressourceCount
            });
            ecb.RemoveComponent<DestinationReached>(entity);
        }
        
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
