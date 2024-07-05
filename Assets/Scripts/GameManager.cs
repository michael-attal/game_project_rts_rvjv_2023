using Unity.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public string SelectedRace { get; set; }
    public float DifficultyLevel { get; set; }
    public string PlayerName { get; set; }

    private void Start()
    {
    }

    private void Update()
    {
    }

    public Difficulty GetDifficulty()
    {
        if (DifficultyLevel < 0.33f)
            return Difficulty.Easy;
        if (DifficultyLevel < 0.66f)
            return Difficulty.Medium;
        return Difficulty.Hard;
    }

    public SpeciesToPlay GetSpeciesToPlay()
    {
        switch (SelectedRace)
        {
            case "Slime":
                return SpeciesToPlay.Slime;
            case "Meca":
                return SpeciesToPlay.Meca;
            default:
                return SpeciesToPlay.Both;
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
}