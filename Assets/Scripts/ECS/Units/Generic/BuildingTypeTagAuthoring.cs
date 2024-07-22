using Unity.Entities;
using UnityEngine;

internal class BuildingTypeTagAuthoring : MonoBehaviour
{
    [SerializeField] public BuildingType typeOfBuilding;

    private class Baker : Baker<BuildingTypeTagAuthoring>
    {
        public override void Bake(BuildingTypeTagAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new BuildingTypeTag
            {
                Type = authoring.typeOfBuilding
            });
        }
    }
}

public enum BuildingType
{
    SlimeBasicWaterUnitBaseSpawner,
    SlimeBasicFireUnitBaseSpawner,
    SlimeBasicEarthUnitBaseSpawner,
    SlimeBasicAirUnitBaseSpawner,

    MecaBasicUnitBaseSpawner
}

public struct BuildingTypeTag : IComponentData
{
    public BuildingType Type;
}