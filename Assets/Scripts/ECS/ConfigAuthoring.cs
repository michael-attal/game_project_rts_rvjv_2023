using Unity.Entities;
using UnityEngine;

// The Config component will be used as a singleton (meaning only one entity will have this component).

public class ConfigAuthoring : MonoBehaviour
{
    [SerializeField] private bool activateSetupGameSystem;
    [SerializeField] private bool activateCleanupGameSystem;
    [SerializeField] private bool activateUnitSpawnerSystem;
    [SerializeField] private bool activateSelectableSystem;
    [SerializeField] private bool activateSelectionRectResizeSystem;
    [SerializeField] private bool activateMoveOrderSystem;
    [SerializeField] private bool activateUnitMovementSystem;
    [SerializeField] private bool activateMovementManualSystem;
    [SerializeField] private bool activateMovementVelocitySystem;
    [SerializeField] private bool activateMovementPositionMotorSystem;
    [SerializeField] private bool activateDestinationReachedCleanupSystem;
    [SerializeField] private bool activateSelectedRendererSystem;
    [SerializeField] private bool activateProjectileRendererSystem;
    [SerializeField] private bool activateUnitAttackSystem;
    [SerializeField] private bool activateUnitDamageSystem;
    [SerializeField] private bool activatePauseScreenSystem;
    [SerializeField] private bool activateWinConditions;
    [SerializeField] private bool activateBuildingScreenSystem;
    [SerializeField] private bool activateGatheringSystem;
    [SerializeField] private bool activateSlimeBasicUnitMergeSystem;
    [SerializeField] private bool activateMecaBasicUnitUpgradeSystem;
    [SerializeField] private bool activateParticleSystems;
    [SerializeField] private FormationType movementFormationType;
    [SerializeField] private bool activateCameraManagerSystem;
    [SerializeField] private bool activateMouseManagerSystem;
    [SerializeField] private bool activateAiManagerSystem;
    [SerializeField] private bool activatePlayerManagerSystem;

    private class Baker : Baker<ConfigAuthoring>
    {
        public override void Bake(ConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new Config
            {
                ActivateSetupGameSystem = authoring.activateSetupGameSystem,
                ActivateCleanupGameSystem = authoring.activateCleanupGameSystem,
                ActivateUnitSpawnerSystem = authoring.activateUnitSpawnerSystem,
                ActivateSelectableSystem = authoring.activateSelectableSystem,
                ActivateSelectionRectResizeSystem = authoring.activateSelectionRectResizeSystem,
                ActivateMoveOrderSystem = authoring.activateMoveOrderSystem,
                ActivateUnitMovementSystem = authoring.activateUnitMovementSystem,
                ActivateMovementPositionMotorSystem = authoring.activateMovementPositionMotorSystem,
                ActivateSelectedRendererSystem = authoring.activateSelectedRendererSystem,
                ActivateProjectileRendererSystem = authoring.activateProjectileRendererSystem,
                ActivateUnitAttackSystem = authoring.activateUnitAttackSystem,
                ActivateUnitDamageSystem = authoring.activateUnitDamageSystem,
                ActivatePauseScreenSystem = authoring.activatePauseScreenSystem,
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
                ActivateMouseManagerSystem = authoring.activateMouseManagerSystem,
                ActivateAiManagerSystem = authoring.activateAiManagerSystem,
                ActivatePlayerManagerSystem = authoring.activatePlayerManagerSystem
            });
        }
    }
}

public struct Config : IComponentData
{
    public bool ActivateSetupGameSystem;
    public bool ActivateCleanupGameSystem;
    public bool ActivateUnitSpawnerSystem;
    public bool ActivateSelectableSystem;
    public bool ActivateSelectionRectResizeSystem;
    public bool ActivateMoveOrderSystem;
    public bool ActivateUnitMovementSystem;
    public bool ActivateMovementManualSystem;
    public bool ActivateMovementVelocitySystem;
    public bool ActivateMovementPositionMotorSystem;
    public bool ActivateDestinationReachedCleanupSystem;
    public bool ActivateSelectedRendererSystem;
    public bool ActivateProjectileRendererSystem;
    public bool ActivateUnitAttackSystem;
    public bool ActivateUnitDamageSystem;
    public bool ActivatePauseScreenSystem;
    public bool ActivateWinConditions;
    public bool ActivateBuildingScreenSystem;
    public bool ActivateGatheringSystem;
    public bool ActivateSlimeBasicUnitMergeSystem;
    public bool ActivateMecaBasicUnitUpgradeSystem;
    public bool ActivateParticleSystems;
    public FormationType MovementFormationType;
    public bool ActivateCameraManagerSystem;
    public bool ActivateMouseManagerSystem;
    public bool ActivateAiManagerSystem;
    public bool ActivatePlayerManagerSystem;
}