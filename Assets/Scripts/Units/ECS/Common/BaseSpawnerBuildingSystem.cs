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
        state.RequireForUpdate<SpawnManager>();
        state.RequireForUpdate<Player>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // We only want to spawn base spawners players one time. Disabling the system stops subsequent updates.
        state.Enabled = false;

        var spawnManager = SystemAPI.GetSingleton<SpawnManager>();

        foreach (var playerInfos in
                 SystemAPI.Query<RefRO<Player>>()
                     .WithAll<Player>())
        {
            var rand = new Random(playerInfos.ValueRO.PlayerNumber);

            var offsetBaseSpawnerBuilding = playerInfos.ValueRO.NbOfBaseSpawnerBuilding == 1
                ? 0
                : rand.NextFloat(playerInfos.ValueRO.NbOfBaseSpawnerBuilding);

            var baseSpawnerPlayer =
                state.EntityManager.Instantiate(playerInfos.ValueRO.Species == Species.Slime
                    ? spawnManager.SlimeBaseSpawnerBuildingPrefab
                    : spawnManager.MecaBaseSpawnerBuildingPrefab);

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

            state.EntityManager.SetComponentData(baseSpawnerPlayer, new BaseSpawnerBuilding
            {
                species = playerInfos.ValueRO.Species
            });
        }

        Debug.Log("Players base unit spawners building successfully created!");
    }
}