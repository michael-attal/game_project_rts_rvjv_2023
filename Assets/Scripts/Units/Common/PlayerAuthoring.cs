using Unity.Entities;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    private class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<Player>(entity);

            AddComponent<Winner>(entity);
            SetComponentEnabled<Winner>(entity, false);
        }
    }
}

public struct Player : IComponentData
{
}

public struct Winner : IComponentData, IEnableableComponent
{
}