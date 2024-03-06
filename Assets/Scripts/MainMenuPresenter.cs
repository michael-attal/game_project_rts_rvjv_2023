using UnityEngine;
using UnityEngine.UI;

public class MainMenuPresenter : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    private void Start()
    {
        startButton.onClick
            .AddListener(StartGame);

        quitButton.onClick
            .AddListener(QuitGame);
    }

    private void StartGame()
    {
        Debug.Log("Loading battlefield scene...");
        var gameManager = GameObject.Find("GameManager");
        var sceneLoader = gameManager.GetComponent<SceneLoader>();
        sceneLoader.sceneToLoad = Scenes.BattlefieldScene;
        StartCoroutine(sceneLoader.LoadSceneAsync());
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}