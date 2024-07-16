using UnityEngine;
using UnityEngine.UI;

public class MainMenuPresenter : MonoBehaviour
{
    [SerializeField] private Button quitButton;
    private SoundManagerMono soundManager;

    private void Start()
    {
        quitButton.onClick
            .AddListener(QuitGame);

        soundManager = GameObject.Find("GameManager").GetComponent<SoundManagerMono>();
        if (soundManager != null)
        {
            soundManager.PlayBackgroundMusic("UIMenu");
        }
        else
        {
            Debug.LogWarning("Error: Cannot find SoundManagerMono");
        }
    }


    private void QuitGame()
    {
        Application.Quit();
    }
}