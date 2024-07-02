using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateAfter(typeof(WinScreenSystem))]
public partial struct CleanupGameSystem : ISystem
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
        var gameManager = SystemAPI.GetSingleton<Game>();

        if (!configManager.ActivateCleanupGameSystem)
        {
            state.Enabled = false;
            return;
        }

        if (gameManager.State == GameState.Over)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (unit, e) in SystemAPI.Query<RefRO<Unit>>().WithEntityAccess()
                    )
            {
                ecb.DestroyEntity(e);
            }

            foreach (var (building, e) in SystemAPI.Query<RefRO<Building>>().WithEntityAccess()
                    )
            {
                ecb.DestroyEntity(e);
            }

            foreach (var (player, e) in SystemAPI.Query<RefRO<Player>>().WithEntityAccess()
                    )
            {
                ecb.DestroyEntity(e);
            }

            foreach (var (projectile, e) in SystemAPI.Query<RefRO<Projectile>>().WithEntityAccess()
                    )
            {
                ecb.DestroyEntity(e);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}