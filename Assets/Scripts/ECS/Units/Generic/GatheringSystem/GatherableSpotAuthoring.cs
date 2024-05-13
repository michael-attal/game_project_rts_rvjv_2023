using Unity.Entities;
using UnityEngine;

public class GatherableSpotAuthoring : MonoBehaviour
{
    [SerializeField] private int ressourceAmount;
    
    private class Baker : Baker<GatherableSpotAuthoring>
    {
        public override void Bake(GatherableSpotAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent(entity, new GatherableSpot()
            {
                RessourceAmount = authoring.ressourceAmount
            });
        }
    }
}

public struct GatherableSpot : IComponentData
{
    public int RessourceAmount;
}
