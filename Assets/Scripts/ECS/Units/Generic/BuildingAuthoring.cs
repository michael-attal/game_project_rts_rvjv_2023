using Unity.Entities;
using UnityEngine;

public class BuildingAuthoring : MonoBehaviour
{
    public BuildingType BuildingType;

    private class Baker : Baker<BuildingAuthoring>
    {
        public override void Bake(BuildingAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new Building
            {
                BuildingType = authoring.BuildingType
            });
        }
    }
}

public enum BuildingType
{
    SlimeBasicUnitBaseSpawnerBuilding,
    MecaBasicUnitBaseSpawnerBuilding
}

public struct Building : IComponentData
{
    public BuildingType BuildingType;
}

public struct BuildingSelected : IComponentData, IEnableableComponent
{
}