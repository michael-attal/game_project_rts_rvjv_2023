using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    private class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<Player>(entity);
        }
    }
}

public enum Species
{
    Slime,
    Meca
}

public struct Player : IComponentData
{
    public uint PlayerNumber;
    public Species Species;
    public int NbOfBaseSpawnerBuilding;
    public float3 StartPosition;
    public bool Winner; // NOTE: Create a component or use a data ?
}