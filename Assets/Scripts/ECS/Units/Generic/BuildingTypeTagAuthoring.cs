using Unity.Entities;
using UnityEngine;

internal class BuildingTypeTagAuthoring : MonoBehaviour
{
    [SerializeField] public BuildingType Species;

    private class Baker : Baker<BuildingTypeTagAuthoring>
    {
        public override void Bake(BuildingTypeTagAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new BuildingTypeTag
            {
                Type = authoring.Species
            });
        }
    }
}

public enum BuildingType
{
    SlimeBaseSpawner,
    MecaBaseSpawner
}

public struct BuildingTypeTag : IComponentData
{
    public BuildingType Type;
}