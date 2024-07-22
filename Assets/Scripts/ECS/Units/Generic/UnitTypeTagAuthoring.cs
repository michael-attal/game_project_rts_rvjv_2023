using Unity.Entities;
using UnityEngine;

internal class UnitTypeTagAuthoring : MonoBehaviour
{
    [SerializeField] public UnitType typeOfUnit;

    private class Baker : Baker<UnitTypeTagAuthoring>
    {
        public override void Bake(UnitTypeTagAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new UnitTypeTag
            {
                Type = authoring.typeOfUnit
            });
        }
    }
}

public enum UnitType
{
    SlimeBasicWaterUnit,
    SlimeBasicFireUnit,
    SlimeBasicEarthUnit,
    SlimeBasicAirUnit,
    SlimeStrongerWaterUnit,
    SlimeStrongerMudUnit,
    SlimeStrongerFirestormUnit,
    SlimeStrongerHydrostormUnit,
    SlimeStrongerMagmaUnit,
    MecaBasicUnit,
    MecaGlassCannonUnit,
    MecaArtilleryUnit,
    MecaGatlingUnit,
    MecaScoutUnit
}

public struct UnitTypeTag : IComponentData
{
    public UnitType Type;
}