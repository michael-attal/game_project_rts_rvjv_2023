using Unity.Entities;
using UnityEngine;

public class SlimeBasicUnitAuthoring : MonoBehaviour
{
    [SerializeField] private FusionInfo FusionInfo;
    
    private class Baker : Baker<SlimeBasicUnitAuthoring>
    {
        public override void Bake(SlimeBasicUnitAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            // A single authoring component can add multiple components to the entity.
            AddComponent<SlimeBasicUnit>(entity);
            AddComponent(entity, new SlimeBasicUnitMerge()
            {
                FusionInfo = authoring.FusionInfo
            });
        }
    }
}

// A tag component for basic slime unit entities.
public struct SlimeBasicUnit : IComponentData
{
}


public struct SlimeBasicUnitMerge : IComponentData
{
    public FusionInfo FusionInfo;
}