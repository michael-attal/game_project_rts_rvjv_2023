using Unity.Entities;
using UnityEngine;

internal class SpeciesTagAuthoring : MonoBehaviour
{
    [SerializeField] public SpeciesType species;

    private class Baker : Baker<SpeciesTagAuthoring>
    {
        public override void Bake(SpeciesTagAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new SpeciesTag
            {
                Type = authoring.species
            });
        }
    }
}

public enum SpeciesType
{
    Slime,
    Meca
}

public struct SpeciesTag : IComponentData
{
    public SpeciesType Type;
}