using Unity.Entities;
using Unity.Rendering;
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
            AddComponent<UnitSelectable>(entity);
            AddComponent<UnitMovement>(entity);
            AddComponent<UnitDamage>(entity);
            AddComponent<UnitAttack>(entity);
            AddComponent<Velocity>(entity);
            AddComponent<Player>(entity);
            AddComponent<URPMaterialPropertyBaseColor>(entity);
        }
    }
}

// A tag component for basic meca unit entities.
public struct MecaBasicUnit : IComponentData
{
}