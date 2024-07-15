using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[UpdateAfter(typeof(MovementSystemGroup))]
internal partial struct SeekRessourceSystem : ISystem
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
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}