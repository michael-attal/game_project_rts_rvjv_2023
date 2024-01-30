using Unity.Entities;
using UnityEngine;

public class BasicSlimeUnitAuthoring : MonoBehaviour
{
    private class Baker : Baker<BasicSlimeUnitAuthoring>
    {
        public override void Bake(BasicSlimeUnitAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            // A single authoring component can add multiple components to the entity.
            AddComponent<BasicSlimeUnit>(entity);
            AddComponent<BasicSlimeUnitMerge>(entity);

            AddComponent<Unit>(entity);
            AddComponent<UnitSelectable>(entity);
            AddComponent<UnitMovement>(entity);
            AddComponent<UnitDamage>(entity);
            AddComponent<UnitAttack>(entity);
            AddComponent<Velocity>(entity);
            AddComponent<Player>(entity);
        }
    }
}

// A tag component for basic slime unit entities.
public struct BasicSlimeUnit : IComponentData
{
}


public struct BasicSlimeUnitMerge : IComponentData
{
}