using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuPresenter : MonoBehaviour
{
    [SerializeField] private Button applyButton;
    [SerializeField] private Slider graphicQualityLevelSlider;
    [SerializeField] private Slider soundLevelSlider;
    [SerializeField] private TMP_Dropdown selectionLanguageDropdown;

    private void Start()
    {
        applyButton.onClick
            .AddListener(ApplySettings);
    }

    private void ApplySettings()
    {
        Debug.Log("Applying settings");

        var selectedLanguageIndex = selectionLanguageDropdown.value;
        var selectedLanguage = selectionLanguageDropdown.options[selectedLanguageIndex].text;
        Debug.Log($"Selected Language Index: {selectedLanguageIndex}");
        Debug.Log($"Selected Language: {selectedLanguage}");

        var graphicQualityLevel = graphicQualityLevelSlider.value;
        Debug.Log($"Graphic Quality Level: {graphicQualityLevel}");

        var soundLevel = soundLevelSlider.value;
        Debug.Log($"Sound Level: {soundLevel}");

        var gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameManager.GraphicQualityLevel = (int)graphicQualityLevel;
        gameManager.SoundLevel = soundLevel;
        gameManager.SelectedLanguage = selectedLanguage;
    }
}