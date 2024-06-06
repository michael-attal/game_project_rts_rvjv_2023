using UnityEngine;

public class PauseScreenSingleton : MonoBehaviour
{
    public static PauseScreenPresenter Instance;
    [SerializeField] private PauseScreenPresenter pauseScreen;

    private void Awake()
    {
        Instance = pauseScreen;
    }
}