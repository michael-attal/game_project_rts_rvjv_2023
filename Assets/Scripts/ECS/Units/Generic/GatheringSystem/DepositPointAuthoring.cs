using Unity.Entities;
using UnityEngine;

class DepositPointAuthoring : MonoBehaviour
{
    private class Baker : Baker<DepositPointAuthoring>
    {
        public override void Bake(DepositPointAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent<DepositPoint>(entity);
        }
    }
}
