using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

partial struct UpgradeScreenSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Game>();
        state.RequireForUpdate<SpawnerUpgradesRegister>();
        state.RequireForUpdate<UnitSelected>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var gameSingleton = SystemAPI.GetSingletonEntity<Game>();

        if (gameSingleton == Entity.Null)
            return;
        
        if (!SystemAPI.HasComponent<SpawnerUpgradesRegister>(gameSingleton))
            return;

        var upgrades = SystemAPI.GetComponent<SpawnerUpgradesRegister>(gameSingleton);

        var ecs = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>()
                     .WithAll<BaseSpawnerBuilding, UnitSelected>()
                     .WithEntityAccess())
        {
            ecs.AddComponent(entity, upgrades);
        }
        
        ecs.RemoveComponent<SpawnerUpgradesRegister>(gameSingleton);
        
        ecs.Playback(state.EntityManager);
        ecs.Dispose();
    }
}
