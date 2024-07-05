using UnityEngine;
using UnityEngine.UI;

public class MainMenuPresenter : MonoBehaviour
{
    [SerializeField] private Button quitButton;

    private void Start()
    {
        quitButton.onClick
            .AddListener(QuitGame);
    }


    private void QuitGame()
    {
        Application.Quit();
    }
}