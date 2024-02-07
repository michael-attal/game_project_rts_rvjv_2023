using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

public class UnitAuthoring : MonoBehaviour
{
    public SpeciesType SpeciesType;
    public float4 UnitColorRGBA;
    public float UnitSpeed;

    private class Baker : Baker<UnitAuthoring>
    {
        public override void Bake(UnitAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new Unit
            {
                SpeciesType = authoring.SpeciesType
            });

            AddComponent(entity, new UnitSelectable
            {
                IsSelected = false,
                ShouldBeSelected = false,
                OriginalUnitColor = authoring.UnitColorRGBA
            });

            AddComponent(entity, new UnitMovement
            {
                Velocity = 10f,
                IsMoving = false,
                Speed = authoring.UnitSpeed
            });

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
    public float4 OriginalUnitColor;
}

public struct UnitMovement : IComponentData
{
    public bool IsMoving;
    public float Speed;
    public float3 Velocity;
    public float3 Destination;
}

public struct IsMovingTag : IComponentData
{
}

public struct UnitDamage : IComponentData
{
}

public struct UnitAttack : IComponentData
{
}