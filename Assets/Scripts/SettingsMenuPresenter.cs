using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuPresenter : MonoBehaviour
{
    [SerializeField] private Button applyButton;
    [SerializeField] private Slider graphicQualityLevelSlider;
    [SerializeField] private Slider soundVolumeSlider;
    [SerializeField] private Slider soundBackgroundVolumeSlider;
    [SerializeField] private TMP_Dropdown selectionLanguageDropdown;

    private SoundManagerMono soundManager;

    private void Start()
    {
        applyButton.onClick.AddListener(ApplySettings);

        soundVolumeSlider.onValueChanged.AddListener(SetSoundVolume);
        soundBackgroundVolumeSlider.onValueChanged.AddListener(SetBackgroundVolume);

        soundManager = GameObject.Find("GameManager").GetComponent<SoundManagerMono>();
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

        var soundVolume = soundVolumeSlider.value;
        Debug.Log($"Sound Volume: {soundVolume}");
        var soundBackgroundVolume = soundBackgroundVolumeSlider.value;
        Debug.Log($"Sound Background Volume: {soundBackgroundVolume}");

        var gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameManager.GraphicQualityLevel = (int)graphicQualityLevel;
        gameManager.SelectedLanguage = selectedLanguage;

        // Appliquer les volumes aux sliders
        soundManager.SetVolume(soundVolume);
        soundManager.SetBackgroundVolume(soundBackgroundVolume);
    }

    private void SetSoundVolume(float volume)
    {
        if (soundManager != null)
        {
            soundManager.SetVolume(volume);
        }
    }

    private void SetBackgroundVolume(float volume)
    {
        if (soundManager != null)
        {
            soundManager.SetBackgroundVolume(volume);
        }
    }
}