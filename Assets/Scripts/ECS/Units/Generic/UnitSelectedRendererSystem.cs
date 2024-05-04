using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(UnitSelectableSystem))]
public partial struct UnitSelectedRendererSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<SpawnManager>();
        state.RequireForUpdate<UnitSelectable>();
    }

    // NOTE: Can't burstcompile : Graphics.DrawMesh 
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();

        if (!configManager.ActivateUnitSelectedRendererSystem)
        {
            state.Enabled = false;
            return;
        }

        var spawnManagerQuery = SystemAPI.QueryBuilder().WithAll<SpawnManager>().Build();
        var spawnManager = spawnManagerQuery.GetSingleton<SpawnManager>();

        foreach (var (unitSelectedTransform, unitSelectable) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<UnitSelectable>>().WithAll<UnitSelected>())
        {
                Graphics.DrawMesh(
                    spawnManager.UnitSelectedCircleMesh,
                    unitSelectedTransform.ValueRO.Position + new float3(0f, -unitSelectedTransform.ValueRO.Position.y + 0.1f, 0f),
                    Quaternion.Euler(90, 0, 0),
                    spawnManager.UnitSelectedCircleMaterial, 0);
        }
    }
}