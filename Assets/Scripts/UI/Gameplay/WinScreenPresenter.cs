using System.Collections;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class WinScreenPresenter : MonoBehaviour
{
    [SerializeField] private GameObject content;
    [SerializeField] private GameObject bottomMenu;
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text winText;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button backToMenuButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private Color slimeColor;
    [SerializeField] private Color mecaColor;

    private EntityQuery entityQuery;

    private void Start()
    {
        entityQuery =
            World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<Game>());

        playAgainButton.onClick.AddListener(PlayAgain);
        backToMenuButton.onClick.AddListener(BackToMenu);
        quitButton.onClick.AddListener(Quit);

        StartCoroutine(WaitForEnd());

        // Ensure WinScreen isn't shown at first
        content.SetActive(false);
    }

    private IEnumerator WaitForEnd()
    {
        while (entityQuery.IsEmpty)
            yield return null;

        Debug.Log("Allez on commence...");
        while (entityQuery.GetSingleton<Game>().State != GameState.Over)
            yield return null;

        DeclareWinner(entityQuery.GetSingleton<Game>().WinningSpecies);
    }

    private void DeclareWinner(SpeciesType winner)
    {
        winText.text = $"The {winner.ToString()}s have won!";
        background.color = winner == SpeciesType.Slime ? slimeColor : mecaColor;

        bottomMenu.SetActive(false);
        content.SetActive(true);
    }

    private void PlayAgain()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        var queryConfigManager = entityManager.CreateEntityQuery(ComponentType.ReadWrite<Game>());

        var gameManagerECS = queryConfigManager.GetSingleton<Game>();
        gameManagerECS.State = GameState.Starting;
        queryConfigManager.SetSingleton(gameManagerECS);

        bottomMenu.SetActive(true);
        content.SetActive(false);

        StartCoroutine(WaitForEnd());
    }

    private void BackToMenu()
    {
        var gameManager = GameObject.Find("GameManager");
        var sceneLoader = gameManager.GetComponent<SceneLoader>();
        sceneLoader.sceneToLoad = Scenes.MenuScene;
        StartCoroutine(sceneLoader.LoadSceneAsync());
    }

    private void Quit()
    {
        Application.Quit();
    }
}