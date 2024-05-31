using AnimCooker;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct UnitAttackSystem : ISystem
{
    private JobHandle dependency;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AnimDbRefData>();
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
        
        var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        
        NativeList<(RefRO<LocalTransform>, RefRO<Unit>, RefRW<UnitDamage>)> targetsList = new NativeList<(RefRO<LocalTransform>, RefRO<Unit>, RefRW<UnitDamage>)>(0, Allocator.Temp);
        foreach (var value in SystemAPI.Query<RefRO<LocalTransform>, RefRO<Unit>, RefRW<UnitDamage>>())
        {
            targetsList.Add(value);
        }

        var array = targetsList.ToArray(Allocator.TempJob);
        targetsList.Dispose();

        var unitAttackJob = new UnitAttackJob()
        {
            ECB = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
            Time = SystemAPI.Time.DeltaTime,
            Targets = array
        };

        var jobHandle = unitAttackJob.ScheduleParallel(dependency);
        jobHandle.Complete();

        array.Dispose();

        dependency = jobHandle;
    }
}

[WithAll(typeof(LocalTransform), typeof(UnitAttack))]
[BurstCompile]
public partial struct UnitAttackJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ECB;
    public float Time;
    public AnimDbRefData AnimDbRefData;
    public NativeArray<(RefRO<LocalTransform>, RefRO<Unit>, RefRW<UnitDamage>)> Targets;

    private void Execute(Entity entity, RefRO<LocalTransform> attackerTransform, RefRO<Unit> attackerInfo, RefRW<UnitAttack> attackerAttack, [ChunkIndexInQuery] int chunkIndex)
    {
        if (attackerAttack.ValueRO.CurrentReloadTime > 0f)
        {
            attackerAttack.ValueRW.CurrentReloadTime -= Time;
            return;
        }

        var attackerPos = attackerTransform.ValueRO.Position;
        RefRW<UnitDamage>? target = null;
        var minimumRange = attackerAttack.ValueRO.Range;

        foreach (var (attackableTransform, attackableInfo, attackableDamage) in Targets)
        {
            if (attackerInfo.ValueRO.SpeciesType == attackableInfo.ValueRO.SpeciesType)
                continue;
            
            var attackablePos = attackableTransform.ValueRO.Position;

            var currentDistance = attackerPos.DistanceTo(attackablePos);
            if (currentDistance <= minimumRange)
            {
                target = attackableDamage;
                minimumRange = currentDistance;
            }
        }

        var modelIndex = AnimDbRefData.FindModelIndex(attackerInfo.ValueRO.BakedPrefabName);

        if (target.HasValue)
        {
            if (!attackerAttack.ValueRO.IsAttackAnimationPlayed && modelIndex >= 0)
            {
                var attackClipIndex = AnimDbRefData.GetModel(modelIndex).FindClipThatContains(GetAnimationNameFromAnimationsType(AnimationsType.Attack));
                if (attackClipIndex < 0)
                {
                    attackClipIndex = 0; // default to first clip if an attack one wasn't found
                }

                ECB.SetComponent(chunkIndex, entity, new AnimationCmdData
                {
                    Cmd = AnimationCmd.PlayOnce, ClipIndex = attackClipIndex
                });
                ECB.SetComponent(chunkIndex, entity, new AnimationSpeedData
                {
                    PlaySpeed = attackerAttack.ValueRO.RateOfFire
                });
                attackerAttack.ValueRW.IsAttackAnimationPlayed = true;
            }

            target.Value.ValueRW.Health -= attackerAttack.ValueRO.Strength;
            attackerAttack.ValueRW.CurrentReloadTime = attackerAttack.ValueRO.RateOfFire;
        }
        else
        {
            if (attackerAttack.ValueRO.IsAttackAnimationPlayed && modelIndex >= 0)
            {
                var idleClipIndex = AnimDbRefData.GetModel(modelIndex).FindClipThatContains(GetAnimationNameFromAnimationsType(AnimationsType.Idle));
                if (idleClipIndex < 0)
                {
                    idleClipIndex = 0;
                }

                // NOTE: Reset animation state
                ECB.SetComponent(chunkIndex, entity, new AnimationCmdData
                {
                    Cmd = AnimationCmd.SetPlayForever, ClipIndex = idleClipIndex
                });
                ECB.SetComponent(chunkIndex, entity, new AnimationSpeedData
                {
                    PlaySpeed = 1f
                });

                attackerAttack.ValueRW.IsAttackAnimationPlayed = false;
            }
        }
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