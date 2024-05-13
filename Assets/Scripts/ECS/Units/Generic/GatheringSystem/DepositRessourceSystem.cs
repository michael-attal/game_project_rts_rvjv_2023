using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateAfter(typeof(SeekDepotSystem))]
partial struct DepositRessourceSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Game>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        
        foreach (var (ressource, entity) in
                 SystemAPI.Query<RefRO<HasRessource>>()
                     .WithAll<DestinationReached, GatheringIntent>()
                     .WithNone<WantsToMove>()
                     .WithEntityAccess())
        {
            var game = SystemAPI.GetSingleton<Game>();
            game.RessourceCount += ressource.ValueRO.CarriedRessources;
            SystemAPI.SetSingleton(game);

            var move = SystemAPI.GetComponent<WantsToMove>(entity);
            Debug.Log($"Depot when destination is {move.Destination.x} {move.Destination.y} {move.Destination.z}");
            
            ecb.RemoveComponent<HasRessource>(entity);
            ecb.SetComponentEnabled<WantsToMove>(entity, false);
            ecb.RemoveComponent<DestinationReached>(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

public struct DepositPoint : IComponentData {}
