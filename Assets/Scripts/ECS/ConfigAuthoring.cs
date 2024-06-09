using Unity.Entities;
using UnityEngine;

// The Config component will be used as a singleton (meaning only one entity will have this component).

public class ConfigAuthoring : MonoBehaviour
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

    public bool ActivateSwitchFocusCameraToPlayersOnSpacePress;

    public static Config UpdateConfigWithPause(Config currentConfig, bool isPaused)
    {
        return new Config
        {
            ActivatePlayerSpawnerSystem = currentConfig.ActivatePlayerSpawnerSystem,
            ActivateBaseSpawnerBuildingSystem = currentConfig.ActivateBaseSpawnerBuildingSystem,
            ActivateUnitSpawnerSystem = currentConfig.ActivateUnitSpawnerSystem,
            ActivateUnitSelectableSystem = currentConfig.ActivateUnitSelectableSystem,
            ActivateSelectionRectResizeSystem = currentConfig.ActivateSelectionRectResizeSystem,
            ActivateMoveOrderSystem = currentConfig.ActivateMoveOrderSystem,
            ActivateUnitMovementSystem = currentConfig.ActivateUnitMovementSystem,
            ActivateMovementPositionMotorSystem = currentConfig.ActivateMovementPositionMotorSystem,
            ActivateUnitSelectedRendererSystem = currentConfig.ActivateUnitSelectedRendererSystem,
            ActivateProjectileRendererSystem = currentConfig.ActivateProjectileRendererSystem,
            ActivateUnitAttackSystem = currentConfig.ActivateUnitAttackSystem,
            ActivateUnitDamageSystem = currentConfig.ActivateUnitDamageSystem,
            ActivatePauseScreenSystem = currentConfig.ActivatePauseScreenSystem,
            IsGamePaused = isPaused, // Here
            ActivateWinConditions = currentConfig.ActivateWinConditions,
            ActivateBuildingScreenSystem = currentConfig.ActivateBuildingScreenSystem,
            ActivateGatheringSystem = currentConfig.ActivateGatheringSystem,
            ActivateSlimeBasicUnitMergeSystem = currentConfig.ActivateSlimeBasicUnitMergeSystem,
            ActivateMecaBasicUnitUpgradeSystem = currentConfig.ActivateMecaBasicUnitUpgradeSystem,
            ActivateDestinationReachedCleanupSystem = currentConfig.ActivateDestinationReachedCleanupSystem,
            ActivateMovementManualSystem = currentConfig.ActivateMovementManualSystem,
            ActivateMovementVelocitySystem = currentConfig.ActivateMovementVelocitySystem,
            ActivateSwitchFocusCameraToPlayersOnSpacePress =
                currentConfig.ActivateSwitchFocusCameraToPlayersOnSpacePress
        };
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
                ActivatePlayerSpawnerSystem = authoring.ActivatePlayerSpawnerSystem,
                ActivateBaseSpawnerBuildingSystem = authoring.ActivateBaseSpawnerBuildingSystem,
                ActivateUnitSpawnerSystem = authoring.ActivateUnitSpawnerSystem,
                ActivateUnitSelectableSystem = authoring.ActivateUnitSelectableSystem,
                ActivateSelectionRectResizeSystem = authoring.ActivateSelectionRectResizeSystem,
                ActivateMoveOrderSystem = authoring.ActivateMoveOrderSystem,
                ActivateUnitMovementSystem = authoring.ActivateUnitMovementSystem,
                ActivateMovementPositionMotorSystem = authoring.ActivateMovementPositionMotorSystem,
                ActivateUnitSelectedRendererSystem = authoring.ActivateUnitSelectedRendererSystem,
                ActivateProjectileRendererSystem = authoring.ActivateProjectileRendererSystem,
                ActivateUnitAttackSystem = authoring.ActivateUnitAttackSystem,
                ActivateUnitDamageSystem = authoring.ActivateUnitDamageSystem,
                ActivatePauseScreenSystem = authoring.ActivatePauseScreenSystem,
                IsGamePaused = authoring.IsGamePaused,
                ActivateWinConditions = authoring.ActivateWinConditions,
                ActivateBuildingScreenSystem = authoring.ActivateBuildingScreenSystem,
                ActivateGatheringSystem = authoring.ActivateGatheringSystem,
                ActivateSlimeBasicUnitMergeSystem = authoring.ActivateSlimeBasicUnitMergeSystem,
                ActivateMecaBasicUnitUpgradeSystem = authoring.ActivateMecaBasicUnitUpgradeSystem,
                ActivateDestinationReachedCleanupSystem = authoring.ActivateDestinationReachedCleanupSystem,
                ActivateMovementManualSystem = authoring.ActivateMovementManualSystem,
                ActivateMovementVelocitySystem = authoring.ActivateMovementVelocitySystem,

                ActivateSwitchFocusCameraToPlayersOnSpacePress =
                    authoring.ActivateSwitchFocusCameraToPlayersOnSpacePress
            });

            if (authoring.ActivateSwitchFocusCameraToPlayersOnSpacePress) AddComponent<ICCamera>(entity);
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

    public bool ActivateSwitchFocusCameraToPlayersOnSpacePress;
}