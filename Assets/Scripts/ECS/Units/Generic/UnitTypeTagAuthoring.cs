using Unity.Entities;
using UnityEngine;

internal class UnitTypeTagAuthoring : MonoBehaviour
{
    [SerializeField] public UnitType Species;

    private class Baker : Baker<UnitTypeTagAuthoring>
    {
        public override void Bake(UnitTypeTagAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new UnitTypeTag
            {
                Type = authoring.Species
            });
        }
    }
}

public enum UnitType
{
    SlimeBasicWaterUnit,
    SlimeStrongerWaterUnit,
    MecaBasicUnit
}

public struct UnitTypeTag : IComponentData
{
    public UnitType Type;
}