using AnimCooker;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using ISystem = Unity.Entities.ISystem;
using SystemState = Unity.Entities.SystemState;

[UpdateAfter(typeof(MovementVelocitySystem))]
[UpdateAfter(typeof(MovementPositionMotorSystem))]
[UpdateAfter(typeof(MovementManualSystem))]
internal partial struct DestinationReachedCleanupSystem : ISystem
{
    private int lastClickID;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var configManager = SystemAPI.GetSingleton<Config>();

        if (!configManager.ActivateDestinationReachedCleanupSystem)
        {
            state.Enabled = false;
            return;
        }

        if (configManager.IsGamePaused)
            return;

        // NOTE: Set Idle animation back when destination reached
        foreach (var (reached, entity) in SystemAPI
                     .Query<RefRO<DestinationReached>>()
                     .WithAll<AnimationCmdData, AnimationSpeedData>()
                     .WithEntityAccess())
        {
            ecb.SetComponent(entity, new AnimationCmdData
            {
                Cmd = AnimationCmd.SetPlayForever, ClipIndex = (short)AnimationsType.Idle
            });
            ecb.SetComponent(entity, new AnimationSpeedData
            {
                PlaySpeed = 1
            });
        }

        // NOTE: Cleanup others components
        foreach (var (reached, entity) in SystemAPI
                     .Query<RefRO<DestinationReached>>()
                     .WithEntityAccess())
        {
            if (SystemAPI.HasComponent<Unit>(entity))
                ecb.SetComponentEnabled<UnitInMovementTag>(entity, false);

            if (!SystemAPI.HasComponent<GatheringIntent>(entity)) // NOTE: Don't remove destination reached for gathering units
                ecb.RemoveComponent<DestinationReached>(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}