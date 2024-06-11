using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

public partial class ParticleSpawningSystem : SystemBase
{
    private Material m_Material;
    private RenderBounds m_QuadBounds;
    private Mesh m_QuadMesh;

    protected override void OnCreate()
    {
        RequireForUpdate<Config>();

        // NOTE: Init basic mesh and material if no one found from Particle Generator
        m_QuadMesh = new Mesh();
        m_QuadMesh.SetVertices(new[]
        {
            new Vector3(-0.5f, -0.5f, 0f),
            new Vector3(-0.5f, 0.5f, 0f),
            new Vector3(0.5f, 0.5f, 0f),
            new Vector3(0.5f, -0.5f, 0f)
        });
        m_QuadMesh.SetTriangles(new[] { 0, 1, 2, 2, 3, 0 }, 0);
        m_QuadMesh.RecalculateNormals();
        m_QuadMesh.RecalculateTangents();
        m_QuadMesh.RecalculateBounds();

        m_QuadBounds = new RenderBounds { Value = m_QuadMesh.bounds.ToAABB() };

        m_Material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
    }


    private void SetupParticleRenderer(ref EntityCommandBuffer ecb, ref Entity particleEntity, float4 color)
    {
        ecb.AddComponent<WorldToLocal_Tag>(particleEntity);
        ecb.AddComponent<WorldRenderBounds>(particleEntity);
        ecb.AddComponent<LocalToWorld>(particleEntity);

        ecb.AddSharedComponent(particleEntity, new RenderFilterSettings
        {
            ShadowCastingMode = ShadowCastingMode.On,
            Layer = 0,
            MotionMode = MotionVectorGenerationMode.Object,
            ReceiveShadows = true,
            RenderingLayerMask = 1,
            StaticShadowCaster = false
        });

        m_Material.SetColor("_BaseColor", new Color(color.x, color.y, color.z, color.w));

        ecb.AddSharedComponentManaged(particleEntity, new RenderMeshArray(new[] { m_Material }, new[] { m_QuadMesh }));
        ecb.AddComponent(particleEntity, MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));
        ecb.AddComponent(particleEntity, m_QuadBounds);

        ecb.AddComponent(particleEntity, new URPMaterialPropertyBaseColor
        {
            Value = color
        });
    }


    private void SetupParticleRenderer(ref EntityCommandBuffer ecb, ref Entity particleEntity, float4 color, in ParticleGeneratorRenderData generatorRenderData)
    {
        ecb.AddComponent<WorldToLocal_Tag>(particleEntity);
        ecb.AddComponent<WorldRenderBounds>(particleEntity);
        ecb.AddComponent<LocalToWorld>(particleEntity);

        ecb.AddSharedComponent(particleEntity, new RenderFilterSettings
        {
            ShadowCastingMode = ShadowCastingMode.On,
            Layer = 0,
            MotionMode = MotionVectorGenerationMode.Object,
            ReceiveShadows = true,
            RenderingLayerMask = 1,
            StaticShadowCaster = false
        });

        var mesh = generatorRenderData.OverrideMesh == null ? m_QuadMesh : generatorRenderData.OverrideMesh;
        var material = generatorRenderData.Material;
        material.SetColor("_BaseColor", new Color(color.x, color.y, color.z, color.w));
        var meshBounds = new RenderBounds { Value = mesh.bounds.ToAABB() };
        ecb.AddSharedComponentManaged(particleEntity, new RenderMeshArray(new[] { material }, new[] { mesh }));
        ecb.AddComponent(particleEntity, MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));
        ecb.AddComponent(particleEntity, meshBounds);

        ecb.AddComponent(particleEntity, new URPMaterialPropertyBaseColor
        {
            Value = color
        });
    }


    protected override void OnUpdate()
    {
        var configManager = SystemAPI.GetSingleton<Config>();

        if (!configManager.ActivateParticleSystems)
        {
            return;
        }

        if (configManager.IsGamePaused)
            return;

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var time = SystemAPI.Time.ElapsedTime;

        foreach (var (particleGeneratorAspect, particleGeneratorRenderData, e) in SystemAPI.Query<ParticleGeneratorAspect, ParticleGeneratorRenderData>().WithEntityAccess())
        {
            if (particleGeneratorAspect.Rate == 0f)
                continue;

            var particlesPerSecond = 1 / particleGeneratorAspect.Rate;

            if (particleGeneratorAspect.LastSpawnTime == 0f || time - particleGeneratorAspect.LastSpawnTime >= particlesPerSecond)
            {
                var particle = ecb.CreateEntity();

                ecb.AddComponent(particle, new Particle
                {
                    Lifetime = particleGeneratorAspect.LifetimeOfParticle
                });

                ecb.AddSharedComponent(particle, new ParticleParent { Value = e });

                ecb.AddComponent(particle, new LocalTransform
                {
                    Position = particleGeneratorAspect.Position,
                    Rotation = quaternion.identity,
                    Scale = particleGeneratorAspect.Size
                });

                var velocity = particleGeneratorAspect.Direction * particleGeneratorAspect.Speed;
                ecb.AddComponent(particle, new ParticleVelocity { Value = velocity });

                SetupParticleRenderer(ref ecb, ref particle, particleGeneratorAspect.Color, particleGeneratorRenderData);

                // NOTE: Update generator info
                particleGeneratorAspect.LastSpawnTime = time;
                particleGeneratorAspect.ParticleCount++;
                particleGeneratorAspect.SpawnedCount++;
            }
        }

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}