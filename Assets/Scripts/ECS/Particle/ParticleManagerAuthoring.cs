using Unity.Entities;
using UnityEngine;

public class ParticleManagerAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject ParticleGeneratorPrefab;

    // [SerializeField] private float rate;
    // [SerializeField] private float lifetime;
    // [SerializeField] private float size;
    // [SerializeField] private float4 Color;
    // [SerializeField] private float Speed;
    //
    // [Header("Renderer")] [SerializeField] private Material Material;
    // [SerializeField] private Mesh Mesh;

    private class Baker : Baker<ParticleManagerAuthoring>
    {
        public override void Bake(ParticleManagerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new ParticleManager
            {
                ParticleGeneratorPrefab = GetEntity(authoring.ParticleGeneratorPrefab, TransformUsageFlags.Dynamic)
            });

            // TODO: Create some basic config and management for all available ParticleGenerator
        }
    }
}

public struct ParticleManager : IComponentData
{
    public Entity ParticleGeneratorPrefab;
}