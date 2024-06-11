using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateAfter(typeof(SeekDepotSystem))]
internal partial struct DepositRessourceSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Game>();
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

        foreach (var (ressource, entity) in
                 SystemAPI.Query<RefRO<HasRessource>>()
                     .WithAll<DestinationReached, GatheringIntent>()
                     .WithNone<WantsToMove>()
                     .WithEntityAccess())
        {
            var game = SystemAPI.GetSingleton<Game>();
            game.RessourceCount += ressource.ValueRO.CarriedRessources;
            SystemAPI.SetSingleton(game);

            ecb.RemoveComponent<HasRessource>(entity);
            ecb.SetComponentEnabled<WantsToMove>(entity, false);
            ecb.RemoveComponent<DestinationReached>(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

public struct DepositPoint : IComponentData
{
}