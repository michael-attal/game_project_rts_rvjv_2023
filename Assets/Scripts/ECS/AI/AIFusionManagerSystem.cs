using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using EntityCommandBuffer = Unity.Entities.EntityCommandBuffer;
using ISystem = Unity.Entities.ISystem;
using SystemAPI = Unity.Entities.SystemAPI;
using SystemState = Unity.Entities.SystemState;

[UpdateInGroup(typeof(AISystemGroup))]
[UpdateAfter(typeof(AIUnitManagerSystem))]
public partial struct AIFusionManagerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Game>();
        state.RequireForUpdate<Config>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();
        var gameManager = SystemAPI.GetSingleton<Game>();

        if (!configManager.ActivateAIManagerSystem)
        {
            state.Enabled = false;
            return;
        }

        if (gameManager.State == GameState.Paused)
            return;

        if (!GameManager.IsAiPlaying(gameManager.SpeciesToPlay))
            return;

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        // Step 1: Select AI units eligible for fusion
        SelectUnitsForFusion(ref ecb, ref state);

        // Step 2: Send fusion orders
        SendFusionOrders(ref ecb, gameManager);

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private void SelectUnitsForFusion(ref EntityCommandBuffer ecb, ref SystemState state)
    {
        // TODO: Iterate over all eligible AI units and activate the WantsToMerge component
    }

    private void SendFusionOrders(ref EntityCommandBuffer ecb, Game gameManager)
    {
        // TODO: Retrieve the fusion recipe for SlimeBasicWaterUnit (or another unit type)

        // TODO: If a recipe is found, calculate the total number of eligible units for fusion

        // TODO: Calculate the number of possible fusions using the total FusionInfo

        // TODO: If fusions are possible, create a FusionOrder
    }

    private int CalculatePossibleFusions(FusionInfo totalFusionInfo, FusionInfo recipeCost)
    {
        // TODO: Replace this logic with the calculation of the number of possible fusions - I don't know if it requires or is done in the merge system.
        return 0;
    }

    private FusionRecipeData? GetFusionRecipeForUnit(UnitType unitType, Game gameManager)
    {
        // TODO: Iterate over the recipes in gameManager and retrieve the recipe corresponding to unitType
        return null;
    }
}