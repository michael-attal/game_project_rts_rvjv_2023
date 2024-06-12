using Unity.Entities;
using UnityEngine;

// The Config component will be used as a singleton (meaning only one entity will have this component).

public class ConfigAuthoring : MonoBehaviour
{
    [SerializeField] private bool activatePlayerSpawnerSystem;
    [SerializeField] private bool activateBaseSpawnerBuildingSystem;
    [SerializeField] private bool activateUnitSpawnerSystem;
    [SerializeField] private bool activateUnitSelectableSystem;
    [SerializeField] private bool activateSelectionRectResizeSystem;
    [SerializeField] private bool activateMoveOrderSystem;
    [SerializeField] private bool activateUnitMovementSystem;
    [SerializeField] private bool activateMovementManualSystem;
    [SerializeField] private bool activateMovementVelocitySystem;
    [SerializeField] private bool activateMovementPositionMotorSystem;
    [SerializeField] private bool activateDestinationReachedCleanupSystem;
    [SerializeField] private bool activateUnitSelectedRendererSystem;
    [SerializeField] private bool activateProjectileRendererSystem;
    [SerializeField] private bool activateUnitAttackSystem;
    [SerializeField] private bool activateUnitDamageSystem;
    [SerializeField] private bool activatePauseScreenSystem;
    [SerializeField] private bool isGamePaused;
    [SerializeField] private bool activateWinConditions;
    [SerializeField] private bool activateBuildingScreenSystem;
    [SerializeField] private bool activateGatheringSystem;
    [SerializeField] private bool activateSlimeBasicUnitMergeSystem;
    [SerializeField] private bool activateMecaBasicUnitUpgradeSystem;
    [SerializeField] private bool activateParticleSystems;
    [SerializeField] private FormationType movementFormationType;
    [SerializeField] private bool activateCameraManagerSystem;
    [SerializeField] private bool activateMouseManagerSystem;

    public static Config UpdateConfigWithPause(Config currentConfig, bool isPaused)
    {
        currentConfig.IsGamePaused = isPaused;
        return currentConfig;
    }

    public static Config UpdateConfigWithToggledPause(Config currentConfig)
    {
        return UpdateConfigWithPause(currentConfig, !currentConfig.IsGamePaused);
    }


    private class Baker : Baker<ConfigAuthoring>
    {
        public override void Bake(ConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new Config
            {
                ActivatePlayerSpawnerSystem = authoring.activatePlayerSpawnerSystem,
                ActivateBaseSpawnerBuildingSystem = authoring.activateBaseSpawnerBuildingSystem,
                ActivateUnitSpawnerSystem = authoring.activateUnitSpawnerSystem,
                ActivateUnitSelectableSystem = authoring.activateUnitSelectableSystem,
                ActivateSelectionRectResizeSystem = authoring.activateSelectionRectResizeSystem,
                ActivateMoveOrderSystem = authoring.activateMoveOrderSystem,
                ActivateUnitMovementSystem = authoring.activateUnitMovementSystem,
                ActivateMovementPositionMotorSystem = authoring.activateMovementPositionMotorSystem,
                ActivateUnitSelectedRendererSystem = authoring.activateUnitSelectedRendererSystem,
                ActivateProjectileRendererSystem = authoring.activateProjectileRendererSystem,
                ActivateUnitAttackSystem = authoring.activateUnitAttackSystem,
                ActivateUnitDamageSystem = authoring.activateUnitDamageSystem,
                ActivatePauseScreenSystem = authoring.activatePauseScreenSystem,
                IsGamePaused = authoring.isGamePaused,
                ActivateWinConditions = authoring.activateWinConditions,
                ActivateBuildingScreenSystem = authoring.activateBuildingScreenSystem,
                ActivateGatheringSystem = authoring.activateGatheringSystem,
                ActivateSlimeBasicUnitMergeSystem = authoring.activateSlimeBasicUnitMergeSystem,
                ActivateMecaBasicUnitUpgradeSystem = authoring.activateMecaBasicUnitUpgradeSystem,
                ActivateDestinationReachedCleanupSystem = authoring.activateDestinationReachedCleanupSystem,
                ActivateMovementManualSystem = authoring.activateMovementManualSystem,
                ActivateMovementVelocitySystem = authoring.activateMovementVelocitySystem,
                ActivateParticleSystems = authoring.activateParticleSystems,
                MovementFormationType = authoring.movementFormationType,
                ActivateCameraManagerSystem = authoring.activateCameraManagerSystem,
                ActivateMouseManagerSystem = authoring.activateMouseManagerSystem
            });
        }
    }
}

public struct Config : IComponentData
{
    public bool ActivatePlayerSpawnerSystem;
    public bool ActivateBaseSpawnerBuildingSystem;
    public bool ActivateUnitSpawnerSystem;
    public bool ActivateUnitSelectableSystem;
    public bool ActivateSelectionRectResizeSystem;
    public bool ActivateMoveOrderSystem;
    public bool ActivateUnitMovementSystem;
    public bool ActivateMovementManualSystem;
    public bool ActivateMovementVelocitySystem;
    public bool ActivateMovementPositionMotorSystem;
    public bool ActivateDestinationReachedCleanupSystem;
    public bool ActivateUnitSelectedRendererSystem;
    public bool ActivateProjectileRendererSystem;
    public bool ActivateUnitAttackSystem;
    public bool ActivateUnitDamageSystem;
    public bool ActivatePauseScreenSystem;
    public bool IsGamePaused;
    public bool ActivateWinConditions;
    public bool ActivateBuildingScreenSystem;
    public bool ActivateGatheringSystem;
    public bool ActivateSlimeBasicUnitMergeSystem;
    public bool ActivateMecaBasicUnitUpgradeSystem;
    public bool ActivateParticleSystems;
    public FormationType MovementFormationType;
    public bool ActivateCameraManagerSystem;
    public bool ActivateMouseManagerSystem;
}