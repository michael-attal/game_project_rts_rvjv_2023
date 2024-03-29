using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

public class UnitAuthoring : MonoBehaviour
{
    public SpeciesType SpeciesType;
    public UnitType UnitType;
    public float4 UnitColorRGBA;
    public float UnitSpeed;

    [Header("Combat")] public float UnitStandardHealth;

    public float UnitAttack;
    public float UnitRange;
    public float UnitRateOfFire;

    private class Baker : Baker<UnitAuthoring>
    {
        public override void Bake(UnitAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new Unit
            {
                SpeciesType = authoring.SpeciesType,
                UnitType = authoring.UnitType
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

            AddComponent(entity, new UnitDamage
            {
                Health = authoring.UnitStandardHealth
            });

            AddComponent(entity, new UnitAttack
            {
                Strength = authoring.UnitAttack,
                Range = authoring.UnitRange,
                RateOfFire = authoring.UnitRateOfFire,
                CurrentReloadTime = 0f
            });

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

public enum UnitType
{
    SlimeBasic,
    MecaBasic,
    SlimeStronger,
    MecaStronger

    // SlimeFire,
    // SlimeWater,
    // ...
}

public struct Unit : IComponentData
{
    public SpeciesType SpeciesType;
    public UnitType UnitType;
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
    public float Health;
}

public struct UnitAttack : IComponentData
{
    public float Strength;
    public float Range;
    public float RateOfFire;
    public float CurrentReloadTime;
}