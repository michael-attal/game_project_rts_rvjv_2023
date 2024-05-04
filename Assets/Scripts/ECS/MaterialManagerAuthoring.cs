using Unity.Entities;
using UnityEngine;

public class MaterialManagerAuthoring : MonoBehaviour
{
    public Material UnitSelectedCircleMaterial;

    private class Baker : Baker<MaterialManagerAuthoring>
    {
        public override void Bake(MaterialManagerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponentObject(entity, new MaterialManager
            {
                UnitSelectedCircleMaterial = authoring.UnitSelectedCircleMaterial
            });
        }
    }
}

public class MaterialManager : IComponentData
{
    public Material UnitSelectedCircleMaterial;
}