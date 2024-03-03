using UnityEngine;

public class WinScreenSingleton : MonoBehaviour
{
    [SerializeField] private WinScreenPresenter winScreen;
    
    public static WinScreenPresenter Instance;
    
    private void Awake()
    {
        Instance = winScreen;
    }
}
