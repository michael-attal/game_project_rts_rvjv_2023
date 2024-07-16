using Unity.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public string SelectedSpecies { get; set; }
    public int DifficultyLevel { get; set; }
    public string PlayerName { get; set; }
    public int GraphicQualityLevel { get; set; }
    public string SelectedLanguage { get; set; }

    public GraphicQuality GetGraphicQualityLevel()
    {
        if (GraphicQualityLevel == 0)
            return GraphicQuality.Low;
        if (GraphicQualityLevel == 1)
            return GraphicQuality.Medium;
        if (GraphicQualityLevel == 2)
            return GraphicQuality.High;
        if (GraphicQualityLevel == 3)
            return GraphicQuality.Ultra;
        return GraphicQuality.Default;
    }

    public Difficulty GetDifficulty()
    {
        if (DifficultyLevel == 0)
            return Difficulty.Easy;
        if (DifficultyLevel == 1)
            return Difficulty.Medium;
        return Difficulty.Hard;
    }

    public SpeciesToPlay GetSpeciesToPlay()
    {
        switch (SelectedSpecies)
        {
            case "Slime":
                return SpeciesToPlay.Slime;
            case "Meca":
                return SpeciesToPlay.Meca;
            default:
                return SpeciesToPlay.Both;
        }
    }

    public Language GetLanguage()
    {
        switch (SelectedLanguage)
        {
            case "English":
                return Language.English;
            case "Français":
                return Language.French;
            default:
                return Language.English;
        }
    }

    public FixedString32Bytes GetPlayerNameAsFixedString()
    {
        return new FixedString32Bytes(PlayerName);
    }

    public static bool IsControlledByCurrentPlayer(SpeciesToPlay currentSpecies, SpeciesType currentEntitySpeciesType)
    {
        return currentSpecies == SpeciesToPlay.Both || (int)currentSpecies == (int)currentEntitySpeciesType;
    }

    public static bool IsControlledByAI(SpeciesToPlay playerSpecies, SpeciesType currentEntitySpeciesType)
    {
        return !IsControlledByCurrentPlayer(playerSpecies, currentEntitySpeciesType);
    }

    public static bool IsAiPlaying(SpeciesToPlay playerSpecies)
    {
        return playerSpecies != SpeciesToPlay.Both;
    }

    public static SpeciesType? GetAiSpecies(SpeciesToPlay playerSpecies)
    {
        switch (playerSpecies)
        {
            case SpeciesToPlay.Slime:
                return SpeciesType.Meca;
            case SpeciesToPlay.Meca:
                return SpeciesType.Slime;
            case SpeciesToPlay.Both:
                return null;
        }

        return null;
    }
}