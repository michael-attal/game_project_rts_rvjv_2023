using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SlimeStrongerWaterUnitAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject ProjectilePrefab;
    [SerializeField] private float3 ProjectileInitialPositionOffset;
    [SerializeField] private float ProjectileScale;
    [SerializeField] private float ProjectileSpeed;
    [SerializeField] private bool IsProjectileAnimated;

    private class Baker : Baker<SlimeStrongerWaterUnitAuthoring>
    {
        public override void Bake(SlimeStrongerWaterUnitAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<SlimeStrongerWaterUnit>(entity);

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

// A tag component for basic slime unit entities.
public struct SlimeStrongerWaterUnit : IComponentData
{
}