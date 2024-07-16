using Unity.Entities;
using UnityEngine;

internal class EntityClassificationTagAuthoring : MonoBehaviour
{
    [SerializeField] public EntityClassification classification;

    private class Baker : Baker<EntityClassificationTagAuthoring>
    {
        public override void Bake(EntityClassificationTagAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new EntityClassificationTag
            {
                Type = authoring.classification
            });
        }
    }
}

public enum EntityClassification
{
    Building,
    Unit,
    Projectile,
    UI,
    Miscellaneous
}

public struct EntityClassificationTag : IComponentData
{
    public EntityClassification Type;
}