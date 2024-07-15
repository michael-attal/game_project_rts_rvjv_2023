using Unity.Entities;
using UnityEngine;

public class MiscellaneousTagAuthoring : MonoBehaviour
{
    private class Baker : Baker<MiscellaneousTagAuthoring>
    {
        public override void Bake(MiscellaneousTagAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<MiscellaneousTag>(entity);
        }
    }
}

public struct MiscellaneousTag : IComponentData
{
}