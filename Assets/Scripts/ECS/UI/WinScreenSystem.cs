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
        if (!SystemAPI.GetSingleton<Config>().ActivateWinConditions)
            state.Enabled = false;

        if (SystemAPI.GetSingleton<Game>().State != GameState.Running)
            return;
        
        // Count remaining units on both sides
        int mecaCount = 0;
        int slimeCount = 0;
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
