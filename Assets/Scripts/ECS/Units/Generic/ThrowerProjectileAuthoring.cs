using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ThrowerProjectileAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject ProjectilePrefab;
    [SerializeField] private float3 ProjectileInitialPositionOffset;
    [SerializeField] private float ProjectileScale;
    [SerializeField] private float ProjectileSpeed;
    [SerializeField] private bool IsProjectileAnimated;

    private class Baker : Baker<ThrowerProjectileAuthoring>
    {
        public override void Bake(ThrowerProjectileAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new ThrowerProjectileInfo
            {
                ProjectilePrefab = GetEntity(authoring.ProjectilePrefab, TransformUsageFlags.Dynamic),
                ProjectileInitialPositionOffset = authoring.ProjectileInitialPositionOffset,
                ProjectileScale = authoring.ProjectileScale,
                Speed = authoring.ProjectileSpeed,
                IsProjectileAnimated = authoring.IsProjectileAnimated
            });

            AddComponent<WantsToThrowProjectile>(entity);
            SetComponentEnabled<WantsToThrowProjectile>(entity, false);
        }
    }
}