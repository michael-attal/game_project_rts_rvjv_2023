using AnimCooker;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(TransformSystemGroup))]
[BurstCompile]
public partial struct UnitAttackSystem : ISystem
{
    // NOTE: Ensure that we don't update every frame the animation
    private bool isIdleAnimationPlayed;
    private bool isAttackAnimationPlayed;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<Game>();
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
        var gameManager = SystemAPI.GetSingleton<Game>();

        if (!configManager.ActivateUnitAttackSystem)
        {
            state.Enabled = false;
            return;
        }

        if (gameManager.State == GameState.Paused)
            return;

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (attackerTransform, attackerSpecies, attackerAttack, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<SpeciesTag>, RefRW<UnitAttack>>().WithAll<UnitAttack>().WithEntityAccess())
        {
            if (attackerAttack.ValueRO.CurrentReloadTime > 0f)
            {
                attackerAttack.ValueRW.CurrentReloadTime -= SystemAPI.Time.DeltaTime;
                continue;
            }

            var attackerPos = attackerTransform.ValueRO.Position;
            RefRW<UnitDamage>? target = null;
            var attackablePos = float3.zero;
            var minimumRange = attackerAttack.ValueRO.Range;

            foreach (var (attackableTransform, attackableSpecies, attackableDamage) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<SpeciesTag>, RefRW<UnitDamage>>().WithAll<UnitDamage>())
            {
                attackablePos = attackableTransform.ValueRO.Position;

                var currentDistance = attackerPos.DistanceTo(attackablePos);
                if (currentDistance <= minimumRange && attackerSpecies.ValueRO.Type != attackableSpecies.ValueRO.Type)
                {
                    target = attackableDamage;
                    minimumRange = currentDistance;
                    break; // NOTE: When a target is find, exit the loop
                }
            }

            if (target.HasValue)
            {
                if (isAttackAnimationPlayed == false && attackerAttack.ValueRO.IsAttackAnimated)
                {
                    ecb.SetComponent(entity, new AnimationCmdData
                    {
                        Cmd = AnimationCmd.PlayOnce, ClipIndex = (short)AnimationsType.Attack, Speed = attackerAttack.ValueRO.RateOfFire
                    });
                    isAttackAnimationPlayed = true;
                    isIdleAnimationPlayed = false;
                }

                if (attackerAttack.ValueRO.UnitAttackType == UnitAttackType.Ranged)
                {
                    ecb.SetComponent(entity, new WantsToThrowProjectile
                    {
                        Destination = attackablePos
                    });
                    ecb.SetComponentEnabled<WantsToThrowProjectile>(entity, true);
                }

                var direction = math.normalize(new float3(attackablePos.x - attackerTransform.ValueRO.Position.x, 0, attackablePos.z - attackerTransform.ValueRO.Position.z));
                ; // Rotate the attacker towards the enemy.
                attackerTransform.ValueRW.Rotation = quaternion.LookRotationSafe(direction, math.up());
                target.Value.ValueRW.Health -= attackerAttack.ValueRO.Strength;
                attackerAttack.ValueRW.CurrentReloadTime = attackerAttack.ValueRO.RateOfFire;
            }
            else
            {
                if (isIdleAnimationPlayed == false && attackerAttack.ValueRO.IsAttackAnimated)
                {
                    // NOTE: Reset animation state
                    ecb.SetComponent(entity, new AnimationCmdData
                    {
                        Cmd = AnimationCmd.SetPlayForever, ClipIndex = (short)AnimationsType.Idle
                    });
                    // NOTE: The doc states that the Speed in AnimationCmdData only works with PlayOnce and PlayOnceAndStop. Therefore, I need to update it in AnimationSpeedData.
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
}