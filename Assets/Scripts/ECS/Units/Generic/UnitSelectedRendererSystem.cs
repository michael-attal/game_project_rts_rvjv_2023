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
        state.RequireForUpdate<MaterialManager>();
        state.RequireForUpdate<UnitSelectable>();
    }

    // NOTE: Can't burstcompile : Graphics.DrawMesh & materialManager
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();

        if (!configManager.ActivateUnitSelectedRendererSystem)
        {
            state.Enabled = false;
            return;
        }

        var materialManagerQuery = SystemAPI.QueryBuilder().WithAll<MaterialManager>().Build();
        var materialManager = materialManagerQuery.GetSingleton<MaterialManager>();

        foreach (var (unitSelectedTransform, unitSelectable) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<UnitSelectable>>().WithAll<UnitSelected>())
        {
            var meshSize = unitSelectedTransform.ValueRO.Scale * 2.2f; // TODO: Create an enum that get the size based on the type of unit or with a Mesh Manager
            Graphics.DrawMesh(
                CreateMesh(meshSize, meshSize),
                unitSelectedTransform.ValueRO.Position + new float3(0f, -unitSelectedTransform.ValueRO.Position.y + 0.2f, 0f),
                Quaternion.Euler(90, 0, 0),
                materialManager.UnitSelectedCircleMaterial, 0);
        }
    }


    public static Mesh CreateMesh(float meshWidth, float meshHeight)
    {
        var vertices = new Vector3[4];
        var uv = new Vector2[4];
        var triangles = new int[6];
        var meshWidthHalf = meshWidth / 2f;
        var meshHeightHalf = meshHeight / 2f;
        vertices[0] = new Vector3(-meshWidthHalf, meshHeightHalf);
        vertices[1] = new Vector3(meshWidthHalf, meshHeightHalf);
        vertices[2] = new Vector3(-meshWidthHalf, -meshHeightHalf);
        vertices[3] = new Vector3(meshWidthHalf, -meshHeightHalf);
        uv[0] = new Vector2(0, 1);
        uv[1] = new Vector2(1, 1);
        uv[2] = new Vector2(0, 0);
        uv[3] = new Vector2(1, 0);
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        triangles[3] = 2;
        triangles[4] = 1;
        triangles[5] = 3;
        var mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        return mesh;
    }
}