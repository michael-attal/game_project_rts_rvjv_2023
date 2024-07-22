using UnityEngine;
using UnityEngine.UI;

public class NavigationButtonsPresenter : MonoBehaviour
{
    [SerializeField] private Button buildingButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button fusionButton;

    private void Start()
    {
        // Check the condition to deactivate the upgrade or merge UI panel
        var gameManagerGameObject = GameObject.Find("GameManager");

        if (gameManagerGameObject != null)
        {
            var gameManagerFromMonobehaviour = gameManagerGameObject.GetComponent<GameManager>();
            if (gameManagerFromMonobehaviour.GetSpeciesToPlay() == SpeciesToPlay.Slime)
            {
                upgradeButton.gameObject.SetActive(false);
            }

            if (gameManagerFromMonobehaviour.GetSpeciesToPlay() == SpeciesToPlay.Meca)
            {
                fusionButton.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.Log("GameManager not detected, all UI will be displayed.");
        }
    }
}