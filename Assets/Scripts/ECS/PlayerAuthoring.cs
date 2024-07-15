using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    [SerializeField] private uint playerNumber;
    [SerializeField] private float3 startPosition;

    [SerializeField] private GameObject baseSpawnerBuildingPrefab;
    [SerializeField] private uint nbOfBaseSpawnerBuilding = 1;

    private class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new Player
            {
                PlayerNumber = authoring.playerNumber,
                StartPosition = authoring.startPosition,
                BaseSpawnerBuildingPrefab = GetEntity(authoring.baseSpawnerBuildingPrefab, TransformUsageFlags.Dynamic),
                NbOfBaseSpawnerBuilding = authoring.nbOfBaseSpawnerBuilding
            });
        }
    }
}

public struct Player : IComponentData
{
    public uint PlayerNumber;
    public float3 StartPosition;

    // public Entity PlayerHandPrefab;
    public Entity BaseSpawnerBuildingPrefab;
    public uint NbOfBaseSpawnerBuilding;
}