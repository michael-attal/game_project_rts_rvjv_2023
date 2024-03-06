using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct UnitAttackSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<UnitAttack>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Implement the shared attack system here.
        // If the Attack system differs significantly between units, we should implement a specialized system, such as MySlimeUnitAttackSystem, in addition of a generic one like this one.

        var configManager = SystemAPI.GetSingleton<Config>();

        if (!configManager.ActivateUnitAttackSystem)
        {
            state.Enabled = false;
            return;
        }
        
        foreach (var (attackerTransform, attackerInfo, attackerAttack) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<Unit>, RefRW<UnitAttack>>().WithAll<UnitAttack>())
        {
            if (attackerAttack.ValueRO.CurrentReloadTime > 0f)
            {
                attackerAttack.ValueRW.CurrentReloadTime -= SystemAPI.Time.DeltaTime;
                continue;
            }
            
            float3 attackerPos = attackerTransform.ValueRO.Position;
            RefRW<UnitDamage>? target = null;
            float minimumRange = attackerAttack.ValueRO.Range;

            foreach (var (attackableTransform, attackableInfo, attackableDamage) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<Unit>, RefRW<UnitDamage>>().WithAll<UnitDamage>())
            {
                float3 attackablePos = attackableTransform.ValueRO.Position;

                float currentDistance = attackerPos.DistanceTo(attackablePos);
                if (currentDistance <= minimumRange && attackerInfo.ValueRO.SpeciesType != attackableInfo.ValueRO.SpeciesType)
                {
                    target = attackableDamage;
                    minimumRange = currentDistance;
                }
            }

            if (target.HasValue)
            {
                target.Value.ValueRW.Health -= attackerAttack.ValueRO.Strength;
                attackerAttack.ValueRW.CurrentReloadTime = attackerAttack.ValueRO.RateOfFire;
            }

        }
    }
}