using Unity.Entities;
using UnityEngine;

public class BuildingAuthoring : MonoBehaviour
{
    private class Baker : Baker<BuildingAuthoring>
    {
        public override void Bake(BuildingAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<Building>(entity);
        }
    }
}

public struct Building : IComponentData
{
}

public struct BuildingSelected : IComponentData, IEnableableComponent
{
}