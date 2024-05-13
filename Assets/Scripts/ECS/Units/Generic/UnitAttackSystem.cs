using AnimCooker;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct UnitAttackSystem : ISystem
{
    // NOTE: Ensure that we don't update every frame the animation
    private bool isIdleAnimationPlayed;
    private bool isAttackAnimationPlayed;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AnimDbRefData>();
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<UnitAttack>();
        isIdleAnimationPlayed = true;
        isAttackAnimationPlayed = false;
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

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (attackerTransform, attackerInfo, attackerAttack, entity) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<Unit>, RefRW<UnitAttack>>().WithAll<UnitAttack>().WithEntityAccess())
        {
            if (attackerAttack.ValueRO.CurrentReloadTime > 0f)
            {
                attackerAttack.ValueRW.CurrentReloadTime -= SystemAPI.Time.DeltaTime;
                continue;
            }

            var attackerPos = attackerTransform.ValueRO.Position;
            RefRW<UnitDamage>? target = null;
            var minimumRange = attackerAttack.ValueRO.Range;

            foreach (var (attackableTransform, attackableInfo, attackableDamage) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<Unit>, RefRW<UnitDamage>>().WithAll<UnitDamage>())
            {
                var attackablePos = attackableTransform.ValueRO.Position;

                var currentDistance = attackerPos.DistanceTo(attackablePos);
                if (currentDistance <= minimumRange && attackerInfo.ValueRO.SpeciesType != attackableInfo.ValueRO.SpeciesType)
                {
                    target = attackableDamage;
                    minimumRange = currentDistance;
                }
            }

            var animDb = SystemAPI.GetSingleton<AnimDbRefData>();
            var modelIndex = animDb.FindModelIndex(attackerInfo.ValueRO.BakedPrefabName);

            if (target.HasValue)
            {
                if (modelIndex >= 0)
                {
                    var idleClipIndex = animDb.GetModel(modelIndex).FindClipThatContains(GetAnimationNameFromAnimationsType(AnimationsType.Attack));
                    if (idleClipIndex < 0)
                    {
                        idleClipIndex = 0; // default to first clip if an idle one wasn't found
                    }

                    if (isAttackAnimationPlayed == false)
                    {
                        ecb.SetComponent(entity, new AnimationCmdData
                        {
                            Cmd = AnimationCmd.PlayOnce, ClipIndex = idleClipIndex
                        });
                        ecb.SetComponent(entity, new AnimationSpeedData
                        {
                            PlaySpeed = attackerAttack.ValueRO.RateOfFire
                        });
                        isAttackAnimationPlayed = true;
                        isIdleAnimationPlayed = false;
                    }
                }
                else
                {
                    Debug.LogWarning($"Attack system error: Could not find model {attackerInfo.ValueRO.BakedPrefabName} in the database.");
                }

                target.Value.ValueRW.Health -= attackerAttack.ValueRO.Strength;
                attackerAttack.ValueRW.CurrentReloadTime = attackerAttack.ValueRO.RateOfFire;
            }
            else
            {
                if (isIdleAnimationPlayed == false && modelIndex >= 0)
                {
                    var idleClipIndex = animDb.GetModel(modelIndex).FindClipThatContains(GetAnimationNameFromAnimationsType(AnimationsType.Idle));
                    if (idleClipIndex < 0)
                    {
                        idleClipIndex = 0;
                    }

                    // NOTE: Reset animation state
                    ecb.SetComponent(entity, new AnimationCmdData
                    {
                        Cmd = AnimationCmd.SetPlayForever, ClipIndex = idleClipIndex
                    });
                    ecb.SetComponent(entity, new AnimationSpeedData
                    {
                        PlaySpeed = 1f
                    });
                    isIdleAnimationPlayed = true;
                    isAttackAnimationPlayed = false;
                }
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private FixedString32Bytes GetAnimationNameFromAnimationsType(AnimationsType animationType)
    {
        switch (animationType)
        {
            case AnimationsType.Idle:
                return new FixedString32Bytes("Idle");

            case AnimationsType.Attack:
                return new FixedString32Bytes("Attack");

            default:
                return new FixedString32Bytes("Idle");
        }
    }
}