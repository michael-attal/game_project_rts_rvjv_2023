using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuPresenter : MonoBehaviour
{
    [SerializeField] private Button applyButton;
    
    void Start()
    {
        applyButton.onClick
            .AddListener(ApplySettings);
    }

    private void ApplySettings()
    {
        Debug.Log("Apply settings here!");
    }
}
