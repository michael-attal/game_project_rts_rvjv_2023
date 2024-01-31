using Unity.Entities;
using UnityEngine;

// NOTE: If we want distinct spawning systems for each species (meca or slime),
// we must create a BaseSlimeSpawnerAuthoring & BaseSlimeSpawnerSystem, for instance, to ensure distinct behaviors for each species.
public class BaseSpawnerBuildingAuthoring : MonoBehaviour
{
    public Species species;

    private class Baker : Baker<BaseSpawnerBuildingAuthoring>
    {
        public override void Bake(BaseSpawnerBuildingAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new BaseSpawnerBuilding
            {
                species = authoring.species
            });
            AddComponent<UnitSpawner>(entity);
            AddComponent<Building>(entity);
            AddComponent<Player>(entity);
        }
    }
}

public struct BaseSpawnerBuilding : IComponentData
{
    public Species species;
}