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
                ecb.AddComponent<GatheringIntent>(entity);
        }
        
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

public struct GatheringIntent : IComponentData {}