using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateAfter(typeof(SeekDepotSystem))]
internal partial struct DepositRessourceSystem : ISystem
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

        foreach (var (ressource, entity) in
                 SystemAPI.Query<RefRO<HasRessource>>()
                     .WithAll<DestinationReached, GatheringIntent>()
                     .WithNone<WantsToMove>()
                     .WithEntityAccess())
        {
            gameManager.RessourceCount += ressource.ValueRO.CarriedRessources;
            SystemAPI.SetSingleton(gameManager);

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