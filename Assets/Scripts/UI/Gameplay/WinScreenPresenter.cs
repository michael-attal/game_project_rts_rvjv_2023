using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinScreenPresenter : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text winText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private Color slimeColor;
    [SerializeField] private Color mecaColor;

    private void Start()
    {
        retryButton.onClick.AddListener(BackToMenu);
        quitButton.onClick.AddListener(Quit);

        // Ensure WinScreen isn't shown at first
        gameObject.SetActive(false);
    }

    public void DeclareWinner(SpeciesType winner)
    {
        winText.text = $"The {winner.ToString()}s have won!";
        background.color = winner == SpeciesType.Slime ? slimeColor : mecaColor;

        gameObject.SetActive(true);
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