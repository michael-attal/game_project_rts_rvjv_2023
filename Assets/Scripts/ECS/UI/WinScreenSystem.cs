using UnityEngine;
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

    [BurstCompile]
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

        var currentGame = SystemAPI.GetSingleton<Game>();
        if (currentGame.State != GameState.Running)
            return;

        int mecaCount = 0;
        int slimeCount = 0;
        foreach (var species in SystemAPI.Query<RefRO<SpeciesTag>>()
                     .WithAll<BaseSpawnerBuilding>())
        {
            if (species.ValueRO.Type == SpeciesType.Slime)
                ++slimeCount;
            else
                ++mecaCount;
        }

        if (mecaCount == 0)
        {
            currentGame.WinningSpecies = SpeciesType.Slime;
            currentGame.State = GameState.Over;
            SystemAPI.SetSingleton(currentGame);
        } else if (slimeCount == 0)
        {
            currentGame.WinningSpecies = SpeciesType.Meca;
            currentGame.State = GameState.Over;
            SystemAPI.SetSingleton(currentGame);
        }
    }
}