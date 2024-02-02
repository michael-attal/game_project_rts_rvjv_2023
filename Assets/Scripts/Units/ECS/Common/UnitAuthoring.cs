using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class UnitAuthoring : MonoBehaviour
{
    private class Baker : Baker<UnitAuthoring>
    {
        public override void Bake(UnitAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<Unit>(entity);

            AddComponent<UnitSelectable>(entity);
            AddComponent<UnitMovement>(entity);
            AddComponent<UnitDamage>(entity);
            AddComponent<UnitAttack>(entity);
            AddComponent<Velocity>(entity);
        }
    }
}

public struct Unit : IComponentData
{
}


// A 2d velocity vector for the unit entities.
public struct Velocity : IComponentData
{
    public float2 Value;
}

public struct UnitSelectable : IComponentData
{
    public bool IsSelected;
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