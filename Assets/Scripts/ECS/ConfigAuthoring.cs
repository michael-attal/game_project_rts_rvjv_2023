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
    public bool ActivateUnitMovementSystem;
    public bool ActivateUnitSelectedRendererSystem;
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
            ActivateUnitMovementSystem = currentConfig.ActivateUnitMovementSystem,
            ActivateUnitSelectedRendererSystem = currentConfig.ActivateUnitSelectedRendererSystem,
            ActivateUnitAttackSystem = currentConfig.ActivateUnitAttackSystem,
            ActivateUnitDamageSystem = currentConfig.ActivateUnitDamageSystem,
            ActivatePauseScreenSystem = currentConfig.ActivatePauseScreenSystem,
            IsGamePaused = isPaused, // Here
            ActivateWinConditions = currentConfig.ActivateWinConditions,
            ActivateGatheringSystem = currentConfig.ActivateBuildingScreenSystem,
            ActivateBuildingScreenSystem = currentConfig.ActivateBuildingScreenSystem,
            ActivateSlimeBasicUnitMergeSystem = currentConfig.ActivateSlimeBasicUnitMergeSystem,
            ActivateMecaBasicUnitUpgradeSystem = currentConfig.ActivateMecaBasicUnitUpgradeSystem,
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
                ActivateUnitMovementSystem = authoring.ActivateUnitMovementSystem,
                ActivateUnitSelectedRendererSystem = authoring.ActivateUnitSelectedRendererSystem,
                ActivateUnitAttackSystem = authoring.ActivateUnitAttackSystem,
                ActivateUnitDamageSystem = authoring.ActivateUnitDamageSystem,
                ActivatePauseScreenSystem = authoring.ActivatePauseScreenSystem,
                IsGamePaused = authoring.IsGamePaused,
                ActivateWinConditions = authoring.ActivateWinConditions,
                ActivateBuildingScreenSystem = authoring.ActivateBuildingScreenSystem,
                ActivateGatheringSystem = authoring.ActivateGatheringSystem,
                ActivateSlimeBasicUnitMergeSystem = authoring.ActivateSlimeBasicUnitMergeSystem,
                ActivateMecaBasicUnitUpgradeSystem = authoring.ActivateMecaBasicUnitUpgradeSystem,

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
    public bool ActivateUnitMovementSystem;
    public bool ActivateUnitSelectedRendererSystem;
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