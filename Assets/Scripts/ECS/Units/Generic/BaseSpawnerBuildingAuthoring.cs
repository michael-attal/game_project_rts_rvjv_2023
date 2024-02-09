using Unity.Entities;
using UnityEngine;

// NOTE: If we want distinct spawning systems for each species (meca or slime),
// we must create a BaseSlimeSpawnerAuthoring & BaseSlimeSpawnerSystem, for instance, to ensure distinct behaviors for each species.
public class BaseSpawnerBuildingAuthoring : MonoBehaviour
{
    public SpeciesType SpeciesType;
    public uint NbOfUnitPerBase = 50; // NOTE: Put 50 by default for the moment
    public GameObject SpawnedUnitPrefab;

    private class Baker : Baker<BaseSpawnerBuildingAuthoring>
    {
        public override void Bake(BaseSpawnerBuildingAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new BaseSpawnerBuilding
            {
                SpeciesType = authoring.SpeciesType,
                NbOfUnitPerBase = authoring.NbOfUnitPerBase,
                SpawnedUnitPrefab =
                    GetEntity(authoring.SpawnedUnitPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct BaseSpawnerBuilding : IComponentData
{
    public SpeciesType SpeciesType;
    public uint NbOfUnitPerBase;
    public Entity SpawnedUnitPrefab;
}