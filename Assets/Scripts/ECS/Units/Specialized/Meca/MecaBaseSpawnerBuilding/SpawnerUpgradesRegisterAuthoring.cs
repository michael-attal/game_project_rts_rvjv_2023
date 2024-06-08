using Unity.Entities;
using UnityEngine;

public class SpawnerUpgradesRegisterAuthoring : MonoBehaviour
{
    public bool debugGlassCannon;
    
    private class Baker : Baker<SpawnerUpgradesRegisterAuthoring>
    {
        public override void Bake(SpawnerUpgradesRegisterAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent(entity, new SpawnerUpgradesRegister()
            {
                HasGlassCannon = authoring.debugGlassCannon
            });
        }
    }
}

public struct SpawnerUpgradesRegister : IComponentData
{
    public bool HasGlassCannon;
}
