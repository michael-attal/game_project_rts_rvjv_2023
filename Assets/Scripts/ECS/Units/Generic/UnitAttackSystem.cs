using AnimCooker;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using ISystem = Unity.Entities.ISystem;
using SystemAPI = Unity.Entities.SystemAPI;
using SystemState = Unity.Entities.SystemState;

[UpdateBefore(typeof(MovementSystemGroup))]
[BurstCompile]
public partial struct UnitAttackSystem : ISystem
{
    // NOTE: Ensure that we don't update every frame the animation
    private bool isIdleAnimationPlayed;
    private bool isAttackAnimationPlayed;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ParticleManager>();
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

        var particleManager = SystemAPI.GetSingleton<ParticleManager>();
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (attackerTransform, attackerSpecies, attackerAttack, attackerSound, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<SpeciesTag>, RefRW<UnitAttack>, RefRW<Sound>>().WithAll<UnitAttack>().WithDisabled<Sound>().WithEntityAccess())
        {
            if (attackerAttack.ValueRO.CurrentReloadTime > 0f)
            {
                attackerAttack.ValueRW.CurrentReloadTime -= SystemAPI.Time.DeltaTime;
                continue;
            }

            var attackerPos = attackerTransform.ValueRO.Position;
            Entity? targetEntity = null;
            RefRW<Damage>? target = null;
            RefRW<Sound>? targetSound = null;
            RefRO<LocalTransform>? targetTransform = null;
            RefRO<EntityClassificationTag>? targetClassification = null;
            var targetSpecies = attackerSpecies.ValueRO.Type == SpeciesType.Slime ? SpeciesType.Meca : SpeciesType.Slime;

            var minimumRange = attackerAttack.ValueRO.Range;

            foreach (var (attackableTransform, attackableSpecies, attackableDamage, attackableSound, attackableClassification, attackableEntity) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<SpeciesTag>, RefRW<Damage>, RefRW<Sound>, RefRO<EntityClassificationTag>>().WithAll<Damage>().WithDisabled<Sound>().WithEntityAccess())
            {
                var attackablePos = attackableTransform.ValueRO.Position;

                var currentDistance = attackerPos.DistanceTo(attackablePos);
                if (currentDistance <= minimumRange && attackerSpecies.ValueRO.Type != attackableSpecies.ValueRO.Type)
                {
                    targetEntity = attackableEntity;
                    target = attackableDamage;
                    targetSound = attackableSound;
                    targetTransform = attackableTransform;
                    targetClassification = attackableClassification;
                    minimumRange = currentDistance;
                    break; // NOTE: When a target is find, exit the loop
                }
            }

            if (target.HasValue)
            {
                var epsilon = 0.0001f;
                var minRateOfFireSpeedAnimation = 0.1f; // NOTE: Minimum expected value for RateOfFire speed animation
                var maxRateOfFireSpeedAnimation = 5.0f; // NOTE: Same but for maximum

                // NOTE: Calculating the inverse with epsilon to avoid division by zero
                var inverseRateOfFire = 1 / (attackerAttack.ValueRO.RateOfFire + epsilon);

                // NOTE: Normalisation of the result between 1 and 10
                var normalizedSpeed = Mathf.Lerp(1, 10, Mathf.InverseLerp(1 / (maxRateOfFireSpeedAnimation + epsilon), 1 / (minRateOfFireSpeedAnimation + epsilon), inverseRateOfFire));

                if (isAttackAnimationPlayed == false && attackerAttack.ValueRO.IsAttackAnimated)
                {
                    ecb.SetComponent(entity, new AnimationCmdData
                    {
                        Cmd = AnimationCmd.PlayOnce,
                        ClipIndex = (short)AnimationsType.Attack,
                        Speed = normalizedSpeed // NOTE: Apply the normalised value to Speed
                    });
                    isAttackAnimationPlayed = true;
                    isIdleAnimationPlayed = false;
                }

                if (attackerAttack.ValueRO.UnitAttackType == UnitAttackType.Ranged)
                {
                    ecb.SetComponent(entity, new WantsToThrowProjectile
                    {
                        Destination = targetTransform.Value.ValueRO.Position
                    });
                    ecb.SetComponentEnabled<WantsToThrowProjectile>(entity, true);
                }

                // NOTE: Play sound for each attack
                if (configManager.ActivateSoundManagerSystem)
                {
                    attackerSound.ValueRW.SoundToPlay = SoundType.Attack;
                    ecb.SetComponentEnabled<Sound>(entity, true);
                }

                var direction = math.normalize(new float3(targetTransform.Value.ValueRO.Position.x - attackerTransform.ValueRO.Position.x, 0, targetTransform.Value.ValueRO.Position.z - attackerTransform.ValueRO.Position.z));
                ; // Rotate the attacker towards the enemy.
                attackerTransform.ValueRW.Rotation = quaternion.LookRotationSafe(direction, math.up());
                target.Value.ValueRW.Health -= attackerAttack.ValueRO.Strength;
                attackerAttack.ValueRW.CurrentReloadTime = attackerAttack.ValueRO.RateOfFire;

                // NOTE: We can remove this condition if we want to apply particles on all targeted units/buildings.
                if (targetClassification.Value.ValueRO.Type == EntityClassification.Building)
                {
                    var particleGenerator = ecb.Instantiate(particleManager.ParticleGeneratorPrefab);
                    var targetScale = targetTransform.Value.ValueRO.Scale;
                    var colorParticles = new float4(0.3f, 0.3f, 0.3f, 0.5f);

                    if (targetSpecies == SpeciesType.Slime)
                    {
                        colorParticles = new float4(0.2f, 0.2f, 1f, 0.5f);
                    }

                    ecb.SetComponent(particleGenerator, new ParticleGeneratorData
                    {
                        Rate = 50f,
                        LifetimeOfGenerator = 0.5f,
                        LifetimeOfParticle = 0.5f,
                        Size = targetScale,
                        Speed = 2f,
                        Direction = new float3(0, 1, 0),
                        Color = colorParticles,
                        IsRandomPositionParticleSpawningActive = true,
                        PositionRangeForRandomParticleSpawning = new float3(1f * targetScale, 1f * targetScale, 1f * targetScale)
                    });
                    ecb.SetComponent(particleGenerator, new LocalTransform
                    {
                        Position = targetTransform.Value.ValueRO.Position,
                        Rotation = quaternion.identity,
                        Scale = 1f
                    });
                }

                if (configManager.ActivateSoundManagerSystem)
                {
                    if (target.Value.ValueRO.Health <= 0)
                    {
                        // NOTE: Play death sound before destroying the entity in DamageSystem
                        targetSound.Value.ValueRW.SoundToPlay = SoundType.Death;
                        ecb.SetComponentEnabled<Sound>(targetEntity.Value, true);
                    }
                    // else
                    // {
                    // NOTE: Currently deactivated but possible, just have to find some good damage audio assets
                    // targetSound.Value.ValueRW.SoundToPlay = SoundType.Damage;
                    // }

                    // ecb.SetComponentEnabled<Sound>(targetEntity.Value, true);
                }
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