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
    public bool ActivateSelectableMaterialChangerSystem;
    public bool ActivateUnitAttackSystem;
    public bool ActivateUnitDamageSystem;

    public bool ActivateSwitchFocusCameraToPlayersOnSpacePress;
    public bool ActivateUnitFollowMousePosition;

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
                ActivateSelectableMaterialChangerSystem = authoring.ActivateSelectableMaterialChangerSystem,
                ActivateUnitAttackSystem = authoring.ActivateUnitAttackSystem,
                ActivateUnitDamageSystem = authoring.ActivateUnitDamageSystem,

                ActivateSwitchFocusCameraToPlayersOnSpacePress =
                    authoring.ActivateSwitchFocusCameraToPlayersOnSpacePress,
                ActivateUnitFollowMousePosition = authoring.ActivateUnitFollowMousePosition
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
    public bool ActivateSelectableMaterialChangerSystem;
    public bool ActivateUnitAttackSystem;
    public bool ActivateUnitDamageSystem;

    public bool ActivateSwitchFocusCameraToPlayersOnSpacePress;
    public bool ActivateUnitFollowMousePosition;
}