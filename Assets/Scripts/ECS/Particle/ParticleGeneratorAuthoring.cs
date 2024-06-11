using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class ParticleGeneratorAuthoring : MonoBehaviour
{
    [SerializeField] private float rate;
    [SerializeField] private float lifetimeOfGenerator;
    [SerializeField] private float lifetimeOfParticle;
    [SerializeField] private float size;
    [SerializeField] private float4 color;
    [SerializeField] private float speed;
    [SerializeField] private float3 direction;

    [Header("Renderer")] [SerializeField] private Material Material;
    [SerializeField] private Mesh Mesh;

    private class Baker : Baker<ParticleGeneratorAuthoring>
    {
        public override void Bake(ParticleGeneratorAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<ParticleGeneratorInfo>(entity);
            AddComponent(entity, new ParticleGeneratorData
            {
                Rate = authoring.rate,
                LifetimeOfParticle = authoring.lifetimeOfParticle,
                Size = authoring.size,
                Color = authoring.color,
                Speed = authoring.speed,
                Direction = authoring.direction
            });

            var renderData = new ParticleGeneratorRenderData(authoring.Material, authoring.Mesh);
            AddSharedComponentManaged(entity, renderData);
        }
    }
}


public struct ParticleGeneratorInfo : IComponentData
{
    public double LastSpawnTime;
    public int ParticleCount; // Current number of living particles spawned by this system
    public int SpawnedCount; // Total count of particles spawned by this particle system since 
}

public struct ParticleGeneratorData : IComponentData
{
    public float Age;
    public float AgeOverLifetime;
    public float Rate;
    public float LifetimeOfGenerator;
    public float LifetimeOfParticle;
    public float Size;
    public float4 Color;
    public float Speed;
    public float3 Direction;
}

public struct ParticleGeneratorRenderData : ISharedComponentData, IEquatable<ParticleGeneratorRenderData>
{
    private int m_Hash;

    public Material Material;

    public Mesh OverrideMesh;


    public ParticleGeneratorRenderData(Material material, Mesh mesh)
    {
        Material = material;
        OverrideMesh = mesh;
        m_Hash = 0;
        RecalculateHash();
    }

    public void RecalculateHash()
    {
        m_Hash = HashCode.Combine(Material, OverrideMesh);
    }

    public bool Equals(ParticleGeneratorRenderData other)
    {
        return other.m_Hash == m_Hash;
    }

    public override bool Equals(object obj)
    {
        return obj is ParticleGeneratorRenderData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return m_Hash;
    }
}

public struct Particle : IComponentData
{
    public float Age;
    public float Lifetime;
    public float AgeOverLifetime;
}

public struct ParticleParent : ICleanupSharedComponentData
{
    public Entity Value;
}

public struct ParticleVelocity : IComponentData
{
    public float3 Value;
}

public readonly partial struct ParticleGeneratorAspect : IAspect
{
    public readonly RefRW<ParticleGeneratorInfo> Info;
    public readonly RefRW<ParticleGeneratorData> Data;
    public readonly RefRW<LocalTransform> Transform;

    public double LastSpawnTime
    {
        get => Info.ValueRO.LastSpawnTime;
        set => Info.ValueRW.LastSpawnTime = value;
    }

    public int ParticleCount
    {
        get => Info.ValueRO.ParticleCount;
        set => Info.ValueRW.ParticleCount = value;
    }

    public int SpawnedCount
    {
        get => Info.ValueRO.SpawnedCount;
        set => Info.ValueRW.SpawnedCount = value;
    }

    public float Age
    {
        get => Data.ValueRO.Age;
        set => Data.ValueRW.Age = value;
    }

    public float AgeOverLifetime
    {
        get => Data.ValueRO.AgeOverLifetime;
        set => Data.ValueRW.AgeOverLifetime = value;
    }

    public float Rate
    {
        get => Data.ValueRO.Rate;
        set => Data.ValueRW.Rate = value;
    }

    public float LifetimeOfGenerator
    {
        get => Data.ValueRO.LifetimeOfGenerator;
        set => Data.ValueRW.LifetimeOfGenerator = value;
    }

    public float LifetimeOfParticle
    {
        get => Data.ValueRO.LifetimeOfParticle;
        set => Data.ValueRW.LifetimeOfParticle = value;
    }

    public float Size
    {
        get => Data.ValueRO.Size;
        set => Data.ValueRW.Size = value;
    }

    public float4 Color
    {
        get => Data.ValueRO.Color;
        set => Data.ValueRW.Color = value;
    }

    public float Speed
    {
        get => Data.ValueRO.Speed;
        set => Data.ValueRW.Speed = value;
    }

    public float3 Direction
    {
        get => Data.ValueRO.Direction;
        set => Data.ValueRW.Direction = value;
    }

    public float3 Position
    {
        get => Transform.ValueRO.Position;
        set => Transform.ValueRW.Position = value;
    }

    public quaternion Rotation
    {
        get => Transform.ValueRO.Rotation;
        set => Transform.ValueRW.Rotation = value;
    }

    public float Scale
    {
        get => Transform.ValueRO.Scale;
        set => Transform.ValueRW.Scale = value;
    }
}