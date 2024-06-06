using AnimCooker;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

public partial struct PauseScreenSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsStep>();
        state.RequireForUpdate<Game>();
        state.RequireForUpdate<Config>();
    }

    // Accessing WinScreenPresenter class, can't use BurstCompile.
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();
        if (!configManager.ActivatePauseScreenSystem)
        {
            state.Enabled = false;
            return;
        }

        if (SystemAPI.GetSingleton<Game>().State != GameState.Running)
            return;

        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        // NOTE: Stop Physics
        var configUnityPhysicsStep = SystemAPI.GetSingleton<PhysicsStep>();
        SystemAPI.SetSingleton(new PhysicsStep
        {
            SimulationType = configUnityPhysicsStep.SimulationType == SimulationType.UnityPhysics ? SimulationType.NoPhysics : SimulationType.UnityPhysics,
            Gravity = configUnityPhysicsStep.Gravity,
            SolverIterationCount = configUnityPhysicsStep.SolverIterationCount,
            SolverStabilizationHeuristicSettings = configUnityPhysicsStep.SolverStabilizationHeuristicSettings,
            MultiThreaded = configUnityPhysicsStep.MultiThreaded,
            SynchronizeCollisionWorld = configUnityPhysicsStep.SynchronizeCollisionWorld
        });

        // NOTE: Stop all animations
        // NOTE: There are many ways to handle this: set the speed to 0 (but if we have a different speed, we must store it in an array to roll it back), set CamData to a higher view to hide the animation, or simply put a black overlay instead of a transparent one (which will be much more performant).
        foreach (var animatedComponent in SystemAPI.Query<RefRW<AnimationSpeedData>>())
        {
            animatedComponent.ValueRW.PlaySpeed = animatedComponent.ValueRW.PlaySpeed <= 0.0001 ? 1f : 0f;
        }

        SystemAPI.SetSingleton(ConfigAuthoring.UpdateConfigWithToggledPause(configManager));
        PauseScreenSingleton.Instance.ToggleDisplayPauseScreen();
    }
}