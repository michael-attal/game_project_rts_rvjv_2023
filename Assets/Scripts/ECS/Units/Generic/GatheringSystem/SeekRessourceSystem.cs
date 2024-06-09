using AnimCooker;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[UpdateAfter(typeof(MovementVelocitySystem))]
[UpdateAfter(typeof(MovementPositionMotor))]
[UpdateAfter(typeof(MovementManualSystem))]
internal partial struct SeekRessourceSystem : ISystem
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

        foreach (var (gatherer, entity) in
                 SystemAPI.Query<RefRO<GatheringIntent>>()
                     .WithNone<HasRessource, WantsToMove, DestinationReached>()
                     .WithEntityAccess())
        {
            var spotLocation = SystemAPI.GetComponent<LocalTransform>(gatherer.ValueRO.AssignedSpot).Position;
            ecb.AddComponent(entity, new WantsToMove
            {
                Destination = spotLocation
            });
            ecb.SetComponentEnabled<WantsToMove>(entity, true);

            // TODO: If we allow buildings to seek ressource adapt this code
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

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}