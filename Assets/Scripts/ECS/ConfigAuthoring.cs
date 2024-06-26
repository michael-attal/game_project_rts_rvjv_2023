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
    public bool ActivateWinConditions;
    public bool ActivateSlimeBasicUnitMergeSystem;
    public bool ActivateMecaBasicUnitUpgradeSystem;

    public bool ActivateSwitchFocusCameraToPlayersOnSpacePress;

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
                ActivateWinConditions = authoring.ActivateWinConditions,
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
    public bool ActivateWinConditions;
    public bool ActivateSlimeBasicUnitMergeSystem;
    public bool ActivateMecaBasicUnitUpgradeSystem;

    public bool ActivateSwitchFocusCameraToPlayersOnSpacePress;
}