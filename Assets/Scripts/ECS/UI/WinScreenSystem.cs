using Unity.Burst;
using Unity.Entities;

public partial struct WinScreenSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Game>();
        state.RequireForUpdate<Config>();
    }

    // Accessing WinScreenPresenter class, can't use BurstCompile.
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();
        if (!configManager.ActivateWinConditions)
        {
            state.Enabled = false;
            return;
        }

        if (configManager.IsGamePaused)
            return;

        if (SystemAPI.GetSingleton<Game>().State != GameState.Running)
            return;

        // NOTE: Count remaining units on both sides (faster with CalculateEntityCount)
        var slimeCount = SystemAPI.QueryBuilder().WithAll<Unit, SlimeUnitTag>().Build().CalculateEntityCount();
        var mecaCount = SystemAPI.QueryBuilder().WithAll<Unit, MecaUnitTag>().Build().CalculateEntityCount();

        // Meca victory
        if (slimeCount == 0)
        {
            WinScreenSingleton.Instance.DeclareWinner(SpeciesType.Meca);
        }
        else if (mecaCount == 0)
        {
            WinScreenSingleton.Instance.DeclareWinner(SpeciesType.Slime);
        }
    }
}