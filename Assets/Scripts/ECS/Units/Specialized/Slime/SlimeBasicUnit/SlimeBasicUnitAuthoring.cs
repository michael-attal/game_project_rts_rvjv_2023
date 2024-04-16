using Unity.Entities;
using UnityEngine;

public class SlimeBasicUnitAuthoring : MonoBehaviour
{
    [SerializeField] private uint NbUnitsToMerge;
    [SerializeField] private UnitType MergedUnitType;
    
    private class Baker : Baker<SlimeBasicUnitAuthoring>
    {
        public override void Bake(SlimeBasicUnitAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            // A single authoring component can add multiple components to the entity.
            AddComponent<SlimeBasicUnit>(entity);
            AddComponent(entity, new SlimeBasicUnitMerge()
            {
                NbUnitsToMerge = authoring.NbUnitsToMerge,
                MergedUnitType = authoring.MergedUnitType
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
    public uint NbUnitsToMerge;
    public UnitType MergedUnitType;
}