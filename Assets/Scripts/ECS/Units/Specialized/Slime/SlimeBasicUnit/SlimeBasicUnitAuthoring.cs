using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public class SlimeBasicUnitAuthoring : MonoBehaviour
{
    private class Baker : Baker<SlimeBasicUnitAuthoring>
    {
        public override void Bake(SlimeBasicUnitAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            // A single authoring component can add multiple components to the entity.
            AddComponent<SlimeBasicUnit>(entity);
            AddComponent<SlimeBasicUnitMerge>(entity);

            // Currently use generic component for generic system but we can create custom one like SlimeUnitSelectable ...
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

// A tag component for basic slime unit entities.
public struct SlimeBasicUnit : IComponentData
{
}


public struct SlimeBasicUnitMerge : IComponentData
{
    public uint NbUnitsToMerge;
    public UnitType MergedUnitType;
}