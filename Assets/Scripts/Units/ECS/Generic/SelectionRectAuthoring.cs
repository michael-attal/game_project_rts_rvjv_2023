using Unity.Entities;
using UnityEngine;

public class SelectionRectAuthoring : MonoBehaviour
{
    public SpeciesType SpeciesType; // If different selection for species
    public bool Active;

    private class Baker : Baker<SelectionRectAuthoring>
    {
        public override void Bake(SelectionRectAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new SelectionRect
            {
                SpeciesType = authoring.SpeciesType,
                Active = authoring.Active
            });

            AddComponent<SelectionRectResize>(entity);
        }
    }
}

public struct SelectionRect : IComponentData
{
    public bool Active;
    public SpeciesType SpeciesType;
}

public struct SelectionRectResize : IComponentData
{
}