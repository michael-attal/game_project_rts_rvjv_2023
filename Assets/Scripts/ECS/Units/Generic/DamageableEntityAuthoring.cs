using Unity.Entities;
using UnityEngine;

public class DamageableEntityAuthoring : MonoBehaviour
{
    [SerializeField] private float EntityBaseHealth;

    private class Baker : Baker<DamageableEntityAuthoring>
    {
        public override void Bake(DamageableEntityAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent(entity, new UnitDamage()
            {
                Health = authoring.EntityBaseHealth
            });
        }
    }
}
