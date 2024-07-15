using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartMenuPresenter : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_Dropdown selectionRaceDropdown;
    [SerializeField] private Slider difficultySlider;
    [SerializeField] private TMP_InputField playerNameInputField;

    private void Start()
    {
        startButton.onClick.AddListener(StartGame);
    }

    private void StartGame()
    {
        Debug.Log("Loading battlefield scene...");

        var selectedRaceIndex = selectionRaceDropdown.value;
        var selectedRace = selectionRaceDropdown.options[selectedRaceIndex].text;
        Debug.Log($"Selected Race Index: {selectedRaceIndex}");
        Debug.Log($"Selected Race: {selectedRace}");

        var difficultyLevel = difficultySlider.value;
        Debug.Log($"Difficulty Level: {difficultyLevel}");

        var playerName = playerNameInputField.text;
        Debug.Log($"Player Name: {playerName}");

        var gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameManager.SelectedRace = selectedRace;
        gameManager.DifficultyLevel = (int)difficultyLevel;
        gameManager.PlayerName = playerName; // Stocker le nom du joueur

        // Changer de scène
        var sceneLoader = gameManager.GetComponent<SceneLoader>();
        sceneLoader.sceneToLoad = Scenes.BattlefieldScene;
        StartCoroutine(sceneLoader.LoadSceneAsync());
    }
}