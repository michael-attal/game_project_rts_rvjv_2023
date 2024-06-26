using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

partial struct GatherRessourceOrderSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<UnitSelectable>();
        state.RequireForUpdate<UnitMovement>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!Input.GetKeyDown(KeyCode.G))
            return;
        
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (transform, entity) in 
                 SystemAPI.Query<RefRO<LocalTransform>>()
                     .WithAll<UnitSelected, UnitMovement>()
                     .WithEntityAccess())
        {
            if (SystemAPI.HasComponent<GatheringIntent>(entity))
                ecb.RemoveComponent<GatheringIntent>(entity);
            else
            {
                float minDistance = float.MaxValue;
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
                    ecb.AddComponent(entity, new GatheringIntent()
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