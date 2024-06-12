using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class UnitAuthoring : MonoBehaviour
{
    public SpeciesType SpeciesType;
    public bool IsMovementAnimated;
    public float UnitSpeed;
    public MovementType MovementType;

    [ConditionalHide("MovementType", (int)MovementType.PositionMotor)]
    public float3 AxisBlockedForMovementPositionMotor;

    [ConditionalHide("MovementType", (int)MovementType.PositionMotor)]
    public float3 PerpendicularAxisForMovementPositionMotor;

    [Header("Combat")] public float UnitStandardHealth;

    public UnitAttackType UnitAttackType;
    public float UnitAttack;
    public float UnitRange;
    public float UnitRateOfFire;
    public bool IsAttackAnimated;

    private class Baker : Baker<UnitAuthoring>
    {
        public override void Bake(UnitAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new Unit
            {
                UnitSpeed = authoring.UnitSpeed
            });

            AddComponent(entity, new UnitSelectable
            {
                ShouldBeSelected = false
            });

            AddComponent<UnitSelected>(entity);
            SetComponentEnabled<UnitSelected>(entity, false);

            switch (authoring.MovementType)
            {
                case MovementType.Manual:
                    AddComponent(entity, new MovementManual
                    {
                        Speed = authoring.UnitSpeed,
                        IsMovementAnimated = authoring.IsMovementAnimated
                    });
                    break;
                case MovementType.Velocity:
                    AddComponent(entity, new MovementVelocity
                    {
                        Speed = authoring.UnitSpeed,
                        IsMovementAnimated = authoring.IsMovementAnimated
                    });
                    break;
                case MovementType.PositionMotor:
                    AddComponent(entity, new MovementPositionMotor
                    {
                        Speed = authoring.UnitSpeed,
                        IsMovementAnimated = authoring.IsMovementAnimated,
                        AxisBlocked = authoring.AxisBlockedForMovementPositionMotor,
                        PerpendicularAxis = authoring.PerpendicularAxisForMovementPositionMotor
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            AddComponent<UnitInMovementTag>(entity);
            SetComponentEnabled<UnitInMovementTag>(entity, false);

            AddComponent<WantsToMove>(entity);
            SetComponentEnabled<WantsToMove>(entity, false);

            AddComponent(entity, new UnitDamage
            {
                Health = authoring.UnitStandardHealth
            });

            AddComponent(entity, new UnitAttack
            {
                UnitAttackType = authoring.UnitAttackType,
                Strength = authoring.UnitAttack,
                Range = authoring.UnitRange,
                RateOfFire = authoring.UnitRateOfFire,
                CurrentReloadTime = 0f,
                IsAttackAnimated = authoring.IsAttackAnimated
            });

            AddComponent<Velocity>(entity);
            AddComponent(entity, new SpeciesTag()
            {
                Type = authoring.SpeciesType
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
    SlimeStrongerWater,
    MecaStronger

    // SlimeFire,
    // SlimeWater,
    // ...
}

public struct SpeciesTag : IComponentData
{
    public SpeciesType Type;
}

public struct Unit : IComponentData
{
    public float UnitSpeed;
}

// A 2d velocity vector for the unit entities.
public struct Velocity : IComponentData
{
    public float2 Value;
}

public struct UnitSelectable : IComponentData
{
    public bool ShouldBeSelected; // If later we want to show an indicator on mouse hover unit
}

public struct UnitSelected : IComponentData, IEnableableComponent
{
}

public enum MovementType
{
    Manual,
    Velocity,
    PositionMotor
}

public struct UnitInMovementTag : IComponentData, IEnableableComponent
{
}

public struct WantsToMove : IComponentData, IEnableableComponent
{
    public float3 Destination;
}

public struct DestinationReached : IComponentData
{
}

public struct UnitDamage : IComponentData
{
    public float Health;
}

public struct UnitAttack : IComponentData
{
    public UnitAttackType UnitAttackType;
    public float Strength;
    public float Range;
    public float RateOfFire;
    public float CurrentReloadTime;
    public bool IsAttackAnimated;
}

public enum UnitAttackType
{
    Melee,
    Ranged
}