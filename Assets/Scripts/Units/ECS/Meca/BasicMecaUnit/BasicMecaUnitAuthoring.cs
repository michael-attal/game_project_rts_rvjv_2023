using Unity.Entities;
using UnityEngine;

public class BasicMecaUnitAuthoring : MonoBehaviour
{
    private class Baker : Baker<BasicMecaUnitAuthoring>
    {
        public override void Bake(BasicMecaUnitAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            // A single authoring component can add multiple components to the entity.
            AddComponent<BasicMecaUnit>(entity);

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

// A tag component for basic meca unit entities.
public struct BasicMecaUnit : IComponentData
{
}


public struct BasicMecaUnitSpawner : IComponentData
{
}