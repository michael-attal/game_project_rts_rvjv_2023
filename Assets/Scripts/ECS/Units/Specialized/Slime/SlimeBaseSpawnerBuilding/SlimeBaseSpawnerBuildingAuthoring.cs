using Unity.Entities;
using UnityEngine;

// NOTE: This is an example of how we can implement a very specific type of base spawner building. - NOT USED ATM
public class SlimeBaseSpawnerBuildingAuthoring : MonoBehaviour
{
    public uint NbOfUnitPerBase = 50; // NOTE: Put 50 by default for the moment
    public GameObject SpawnedUnitPrefab;

    private class Baker : Baker<SlimeBaseSpawnerBuildingAuthoring>
    {
        public override void Bake(SlimeBaseSpawnerBuildingAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new SlimeBaseSpawnerBuilding
            {
                NbOfUnitPerBase = authoring.NbOfUnitPerBase,
                SpawnedUnitPrefab =
                    GetEntity(authoring.SpawnedUnitPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct SlimeBaseSpawnerBuilding : IComponentData
{
    public uint NbOfUnitPerBase;
    public Entity SpawnedUnitPrefab;
}