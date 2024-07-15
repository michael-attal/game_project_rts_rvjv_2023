using Unity.Entities;
using UnityEngine;

public class SelectionRectAuthoring : MonoBehaviour
{
    public bool Active;

    private class Baker : Baker<SelectionRectAuthoring>
    {
        public override void Bake(SelectionRectAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new SelectionRect
            {
                Active = authoring.Active
            });

            AddComponent<SelectionRectResize>(entity);
        }
    }
}

public struct SelectionRect : IComponentData
{
    public bool Active;
}

public struct SelectionRectResize : IComponentData
{
}