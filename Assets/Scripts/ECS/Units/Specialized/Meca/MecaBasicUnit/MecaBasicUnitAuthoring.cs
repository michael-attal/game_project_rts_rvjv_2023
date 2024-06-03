using Unity.Entities;
using UnityEngine;

public class MecaBasicUnitAuthoring : MonoBehaviour
{
    private class Baker : Baker<MecaBasicUnitAuthoring>
    {
        public override void Bake(MecaBasicUnitAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            // A single authoring component can add multiple components to the entity.
            AddComponent<MecaBasicUnit>(entity);

            // Add generic system, but we can implement dedicated system for this component like BasicMecaMovementSystem for example.
        }
    }
}

// A tag component for basic meca unit entities.
public struct MecaBasicUnit : IComponentData
{
}

public struct MecaBasicUnitUpgrade : IComponentData
{
}