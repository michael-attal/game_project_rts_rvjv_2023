using Unity.Entities;
using UnityEngine;

public class SelectionCircleAuthoring : MonoBehaviour
{
    private class Baker : Baker<SelectionCircleAuthoring>
    {
        public override void Bake(SelectionCircleAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new SelectionCircle());
        }
    }
}

public struct SelectionCircle : IComponentData
{
}