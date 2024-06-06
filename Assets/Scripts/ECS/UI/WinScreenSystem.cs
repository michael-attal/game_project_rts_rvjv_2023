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

        // Count remaining units on both sides
        var mecaCount = 0;
        var slimeCount = 0;
        foreach (var unit in SystemAPI.Query<RefRO<Unit>>())
        {
            if (unit.ValueRO.SpeciesType == SpeciesType.Slime)
                slimeCount++;
            else
                mecaCount++;
        }

        // Check victory on both sides
        // Slimes victory
        if (mecaCount == 0)
        {
            WinScreenSingleton.Instance.DeclareWinner(SpeciesType.Slime);
        }

        // Mecas victory
        if (slimeCount == 0)
        {
            WinScreenSingleton.Instance.DeclareWinner(SpeciesType.Meca);
        }
    }
}