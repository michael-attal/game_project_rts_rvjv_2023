using AnimCooker;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;
using UnityEngine.UI;

public class PauseScreenPresenter : MonoBehaviour
{
    [SerializeField] private GameObject bottomMenu;
    [SerializeField] private Button continueButton;

    private void Start()
    {
        continueButton.onClick.AddListener(ContinueGame);

        // Ensure PauseScreen isn't shown at first
        gameObject.SetActive(false);
    }

    public void ToggleDisplayPauseScreen()
    {
        bottomMenu.SetActive(false);
        gameObject.SetActive(!gameObject.activeSelf);
    }

    private void ContinueGame()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        var queryConfigManager = entityManager.CreateEntityQuery(ComponentType.ReadWrite<Config>());

        var queryUnityPhysicsStep = entityManager.CreateEntityQuery(ComponentType.ReadWrite<PhysicsStep>());

        var queryAnimationSpeedData = entityManager.CreateEntityQuery(ComponentType.ReadWrite<AnimationSpeedData>());

        var configManager = queryConfigManager.GetSingleton<Config>();
        queryConfigManager.SetSingleton(ConfigAuthoring.UpdateConfigWithPause(configManager, false));

        var configUnityPhysicsStep = queryUnityPhysicsStep.GetSingleton<PhysicsStep>();
        queryUnityPhysicsStep.SetSingleton(new PhysicsStep
        {
            SimulationType = SimulationType.UnityPhysics,
            Gravity = configUnityPhysicsStep.Gravity,
            SolverIterationCount = configUnityPhysicsStep.SolverIterationCount,
            SolverStabilizationHeuristicSettings = configUnityPhysicsStep.SolverStabilizationHeuristicSettings,
            MultiThreaded = configUnityPhysicsStep.MultiThreaded,
            SynchronizeCollisionWorld = configUnityPhysicsStep.SynchronizeCollisionWorld
        });

        // NOTE: Updating every entity from MonoBehaviour can be slow...
        using (var entities = queryAnimationSpeedData.ToEntityArray(Allocator.TempJob))
        {
            foreach (var entity in entities)
            {
                var animationSpeedData = entityManager.GetComponentData<AnimationSpeedData>(entity);
                animationSpeedData.PlaySpeed = 1f;
                entityManager.SetComponentData(entity, animationSpeedData);
            }
        }

        bottomMenu.SetActive(true);
        gameObject.SetActive(false);
    }
}