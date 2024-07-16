using Unity.Burst;
using Unity.Entities;

public partial struct WinScreenSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SoundManager>();
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<Game>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();
        var gameManager = SystemAPI.GetSingleton<Game>();
        var soundManagerEntity = SystemAPI.GetSingletonEntity<SoundManager>();

        if (!configManager.ActivateWinConditions)
        {
            state.Enabled = false;
            return;
        }

        if (gameManager.State == GameState.Paused)
            return;

        if (gameManager.State != GameState.Running)
            return;

        var mecaCount = 0;
        var slimeCount = 0;
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
            gameManager.WinningSpecies = SpeciesType.Slime;
            gameManager.State = GameState.Over;
            SystemAPI.SetSingleton(gameManager);
        }
        else if (slimeCount == 0)
        {
            gameManager.WinningSpecies = SpeciesType.Meca;
            gameManager.State = GameState.Over;
            SystemAPI.SetSingleton(gameManager);
        }

        if (gameManager.State == GameState.Over)
        {
            var sound = SystemAPI.GetComponentRW<Sound>(soundManagerEntity);

            // NOTE: Because we can play both species, we don't want to hear win or lost sound in that case
            var playWinSound =
                (gameManager.SpeciesToPlay == SpeciesToPlay.Slime && mecaCount == 0) ||
                (gameManager.SpeciesToPlay == SpeciesToPlay.Meca && slimeCount == 0);

            var playLostSound =
                (gameManager.SpeciesToPlay == SpeciesToPlay.Slime && slimeCount == 0) ||
                (gameManager.SpeciesToPlay == SpeciesToPlay.Meca && mecaCount == 0);

            if (playWinSound)
            {
                sound.ValueRW.SoundToPlay = SoundType.Victory;
            }
            else if (playLostSound)
            {
                sound.ValueRW.SoundToPlay = SoundType.Lost;
            }

            SystemAPI.SetComponentEnabled<Sound>(soundManagerEntity, true);
        }
    }
}