using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct BaseSpawnerBuildingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<Player>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();

        if (!configManager.ActivateBaseSpawnerBuildingSystem)
        {
            state.Enabled = false;
            return;
        }

        if (configManager.IsGamePaused)
            return;

        // We only want to spawn base spawners players one time. Disabling the system stops subsequent updates.
        state.Enabled = false;

        foreach (var playerInfos in
                 SystemAPI.Query<RefRO<Player>>()
                     .WithAll<Player>())
        {
            var rand = new Random(playerInfos.ValueRO.PlayerNumber);

            var offsetBaseSpawnerBuilding = playerInfos.ValueRO.NbOfBaseSpawnerBuilding == 1
                ? 0
                : rand.NextFloat(playerInfos.ValueRO.NbOfBaseSpawnerBuilding);

            var baseSpawnerPlayer =
                state.EntityManager.Instantiate(playerInfos.ValueRO.BaseSpawnerBuildingPrefab);

            // Position the new base building spawner by setting its LocalTransform component.
            state.EntityManager.SetComponentData(baseSpawnerPlayer, new LocalTransform
            {
                Position = new float3
                {
                    x = playerInfos.ValueRO.StartPosition.x + offsetBaseSpawnerBuilding,
                    y = playerInfos.ValueRO.StartPosition.y,
                    z = playerInfos.ValueRO.StartPosition.z + offsetBaseSpawnerBuilding
                },
                Scale = 1,
                Rotation = quaternion.identity
            });
        }

        Debug.Log("Players base unit spawners building successfully created!");
    }
}