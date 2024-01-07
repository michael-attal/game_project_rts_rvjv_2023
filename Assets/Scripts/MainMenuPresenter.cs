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
        Debug.Log("Change scene here!");
    }

    private void QuitGame()
    {
        
        Application.Quit();
    }
}
