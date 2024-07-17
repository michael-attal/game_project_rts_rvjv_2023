using Unity.Burst;
using Unity.Entities;

internal partial struct SelectedFusionInfoSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<Game>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();

        if (!configManager.ActivateSlimeBasicUnitMergeSystem)
        {
            state.Enabled = false;
            return;
        }

        // Add together all selected FusionInfo components
        var selectedFusionInfo = new FusionInfo();

        foreach (var merge in SystemAPI.Query<SlimeBasicUnitMerge>()
                     .WithAll<Selected>())
            selectedFusionInfo += merge.FusionInfo;

        // Transfer it to the Game singleton entity
        var gameEntity = SystemAPI.GetSingletonEntity<Game>();
        SystemAPI.SetComponent(gameEntity, new SlimeBasicUnitMerge
        {
            FusionInfo = selectedFusionInfo
        });
    }
}