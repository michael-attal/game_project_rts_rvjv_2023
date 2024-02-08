using Unity.Entities;
using UnityEngine;

// NOTE: This is an example of how we can implement a very specific type of base spawner building. - NOT USED ATM
public class MecaBaseSpawnerBuildingAuthoring : MonoBehaviour
{
    public uint NbOfUnitPerBase = 50; // NOTE: Put 50 by default for the moment
    public GameObject SpawnedUnitPrefab;

    private class Baker : Baker<MecaBaseSpawnerBuildingAuthoring>
    {
        public override void Bake(MecaBaseSpawnerBuildingAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new MecaBaseSpawnerBuilding
            {
                NbOfUnitPerBase = authoring.NbOfUnitPerBase,
                SpawnedUnitPrefab =
                    GetEntity(authoring.SpawnedUnitPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct MecaBaseSpawnerBuilding : IComponentData
{
    public uint NbOfUnitPerBase;
    public Entity SpawnedUnitPrefab;
}