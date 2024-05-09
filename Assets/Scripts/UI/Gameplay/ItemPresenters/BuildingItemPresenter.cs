using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingItemPresenter : MonoBehaviour
{
    public Button Button => button;

    [SerializeField] private TMP_Text title;
    [SerializeField] private Image image;
    [SerializeField] private Button button;

    public void Initialize(BuildingOptionData data)
    {
        title.text = data.title;
        if (data.image)
            image.sprite = data.image;
        else
            image.gameObject.SetActive(false);
    }
}
