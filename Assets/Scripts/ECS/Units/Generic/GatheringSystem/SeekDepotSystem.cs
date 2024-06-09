using AnimCooker;
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
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();
        if (!configManager.ActivateGatheringSystem)
        {
            state.Enabled = false;
            return;
        }

        if (configManager.IsGamePaused)
            return;

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (transform, entity) in
                 SystemAPI.Query<RefRO<LocalTransform>>()
                     .WithAll<HasRessource, GatheringIntent>()
                     .WithNone<WantsToMove, DestinationReached>()
                     .WithEntityAccess())
        {
            var minDistance = float.MaxValue;
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
                ecb.SetComponent(entity, new WantsToMove
                {
                    Destination = minLocation.Value
                });
                ecb.SetComponentEnabled<WantsToMove>(entity, true);
                ecb.RemoveComponent<DestinationReached>(entity);

                // TODO: If we allow buildings to seek depot adapt this code
                if (SystemAPI.HasComponent<Unit>(entity) && SystemAPI.HasComponent<AnimationCmdData>(entity))
                {
                    // NOTE: Start move animation
                    ecb.SetComponent(entity, new AnimationCmdData
                    {
                        Cmd = AnimationCmd.SetPlayForever, ClipIndex = (short)AnimationsType.Move
                    });
                    ecb.SetComponent(entity, new AnimationSpeedData
                    {
                        PlaySpeed = SystemAPI.GetComponent<Unit>(entity).UnitSpeed
                    });
                }
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}