using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(UnitAttackSystem))]
[UpdateBefore(typeof(DamageSystem))]
internal partial struct SoundManagerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SoundManager>();
        state.RequireForUpdate<Game>();
        state.RequireForUpdate<Config>();
    }

    // [BurstCompile] using SoundManagerMonoBehaviour
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingletonRW<Config>();
        var gameManager = SystemAPI.GetSingleton<Game>();
        var soundManager = SystemAPI.GetSingleton<SoundManager>();

        if (!configManager.ValueRO.ActivateSoundManagerSystem)
        {
            state.Enabled = false;
            return;
        }

        if (gameManager.State == GameState.Paused)
            return;

        SystemAPI.SetSingleton(soundManager);

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var gameManagerGameObject = GameObject.Find("GameManager");

        // NOTE: If no sound manager found deactivate the system and deactivate sound IEnableComponent
        if (gameManagerGameObject == null)
        {
            configManager.ValueRW.ActivateSoundManagerSystem = false;
            foreach (var (transform, sound, entity) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<Sound>>().WithEntityAccess())
            {
                SystemAPI.SetComponentEnabled<Sound>(entity, false);
            }

            return;
        }

        var gameManagerFromMonobehaviour = gameManagerGameObject.GetComponent<SoundManagerMono>();

        if (gameManagerFromMonobehaviour == null) return;

        foreach (var (transform, sound, entity) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<Sound>>().WithEntityAccess())
        {
            gameManagerFromMonobehaviour.PlaySound(sound.ValueRO.EntityType, sound.ValueRO.SoundToPlay, transform.ValueRO.Position);
            ecb.SetComponentEnabled<Sound>(entity, false);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}