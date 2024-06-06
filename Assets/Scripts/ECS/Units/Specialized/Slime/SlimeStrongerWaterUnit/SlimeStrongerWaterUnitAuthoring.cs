using Unity.Entities;
using UnityEngine;

public class SlimeStrongerWaterUnitAuthoring : MonoBehaviour
{
    private class Baker : Baker<SlimeStrongerWaterUnitAuthoring>
    {
        public override void Bake(SlimeStrongerWaterUnitAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<SlimeStrongerWaterUnit>(entity);
        }
    }
}

// A tag component for basic slime unit entities.
public struct SlimeStrongerWaterUnit : IComponentData
{
}