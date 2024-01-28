using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BasicSlimeAuthoring : MonoBehaviour
{
    private class Baker : Baker<BasicSlimeAuthoring>
    {
        public override void Bake(BasicSlimeAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            // A single authoring component can add multiple components to the entity.
            AddComponent<BasicSlime>(entity);
            AddComponent<Velocity>(entity);

            AddComponent<Winner>(entity);
            SetComponentEnabled<Winner>(entity, false);
        }
    }
}

// A tag component for basic slime entities.
public struct BasicSlime : IComponentData
{
}

// A 2d velocity vector for the basic slime entities.
public struct Velocity : IComponentData
{
    public float2 Value;
}