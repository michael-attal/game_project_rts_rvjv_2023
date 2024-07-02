using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// NOTE: If we want distinct spawning systems for each species (meca or slime),
// we must create a BaseSlimeSpawnerAuthoring & BaseSlimeSpawnerSystem, for instance, to ensure distinct behaviors for each species.
public class BaseSpawnerBuildingAuthoring : MonoBehaviour
{
    public uint NbOfUnitPerBase = 50; // NOTE: Put 50 by default for the moment
    public float GenerationInterval;
    public GameObject SpawnedUnitPrefab;
    public float3 SpawnedUnitInitialPosition;
    public quaternion SpawnedUnitInitialRotation;
    public float SpawnedUnitInitialScale;
    public SpeciesType Species;

    private class Baker : Baker<BaseSpawnerBuildingAuthoring>
    {
        public override void Bake(BaseSpawnerBuildingAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new BaseSpawnerBuilding
            {
                NbOfUnitPerBase = authoring.NbOfUnitPerBase,
                GenerationInterval = authoring.GenerationInterval,
                TimeToNextGeneration = 0f,
                SpawnedUnitPrefab =
                    GetEntity(authoring.SpawnedUnitPrefab, TransformUsageFlags.Dynamic),
                UnitInitialRotation = authoring.SpawnedUnitInitialRotation,
                UnitInitialPosition = authoring.SpawnedUnitInitialPosition,
                UnitInitialScale = authoring.SpawnedUnitInitialScale
            });
            AddComponent<UnitSelected>(entity);
            SetComponentEnabled<UnitSelected>(entity, false);
            AddComponent<UnitSelectable>(entity);

            AddComponent(entity, new SpeciesTag
            {
                Type = authoring.Species
            });
        }
    }
}

public struct BaseSpawnerBuilding : IComponentData
{
    public uint NbOfUnitPerBase;
    public float GenerationInterval;
    public float TimeToNextGeneration;
    public Entity SpawnedUnitPrefab;
    public float3 UnitInitialPosition;
    public Quaternion UnitInitialRotation;
    public float UnitInitialScale;
}