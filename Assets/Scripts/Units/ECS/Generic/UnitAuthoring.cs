using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

public class UnitAuthoring : MonoBehaviour
{
    public SpeciesType SpeciesType;
    public float4 UnitColorRGBA;

    private class Baker : Baker<UnitAuthoring>
    {
        public override void Bake(UnitAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new Unit
            {
                SpeciesType = authoring.SpeciesType
            });

            AddComponent<UnitSelectable>(entity);
            AddComponent<UnitMovement>(entity);
            AddComponent<UnitDamage>(entity);
            AddComponent<UnitAttack>(entity);
            AddComponent<Velocity>(entity);

            AddComponent(entity, new URPMaterialPropertyBaseColor
            {
                Value = authoring.UnitColorRGBA
            });
        }
    }
}

public enum SpeciesType
{
    Slime,
    Meca
}

public struct Unit : IComponentData
{
    public SpeciesType SpeciesType;
}

// A 2d velocity vector for the unit entities.
public struct Velocity : IComponentData
{
    public float2 Value;
}

public struct UnitSelectable : IComponentData
{
    public bool IsSelected;
    public bool ShouldBeSelected;
}

public struct UnitMovement : IComponentData
{
}

public struct UnitDamage : IComponentData
{
}

public struct UnitAttack : IComponentData
{
}