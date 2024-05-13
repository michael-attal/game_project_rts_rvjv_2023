using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class UnitAuthoring : MonoBehaviour
{
    public SpeciesType SpeciesType;
    public UnitType UnitType;
    public string BakedPrefabName;
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
                UnitType = authoring.UnitType,
                BakedPrefabName = new FixedString32Bytes(authoring.BakedPrefabName)
            });

            AddComponent(entity, new UnitSelectable
            {
                ShouldBeSelected = false
            });

            AddComponent<UnitSelected>(entity);
            SetComponentEnabled<UnitSelected>(entity, false);

            AddComponent(entity, new UnitMovement
            {
                Speed = authoring.UnitSpeed
            });

            AddComponent<WantsToMove>(entity);
            SetComponentEnabled<WantsToMove>(entity, false);

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
        }
    }
}

public enum SpeciesType
{
    Slime,
    Meca
}

// NOTE: It's important that each prefab has the same order for the animations
// TODO: Refactoring to make it more open and easy to modify
public enum AnimationsType
{
    Idle,
    Attack
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
    public FixedString32Bytes BakedPrefabName;
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

public struct UnitMovement : IComponentData
{
    public float Speed;
}

public struct WantsToMove : IComponentData, IEnableableComponent
{
    public float3 Destination;
}

public struct DestinationReached : IComponentData {}

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