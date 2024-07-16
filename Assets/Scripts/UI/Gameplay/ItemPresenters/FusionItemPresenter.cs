using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FusionItemPresenter : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private Image image;
    [SerializeField] private Button button;

    public FusionInfo CurrentFusionInfo
        => currentDescriptor.FusionInfo;

    public Button Button => button;
    
    private FusionDescriptor currentDescriptor;

    public void Present(FusionDescriptor newDescriptor)
    {
        currentDescriptor = newDescriptor;

        if (nameText)
            nameText.text = newDescriptor.DisplayName;
        if (image && newDescriptor.DisplayImage)
            image.sprite = newDescriptor.DisplayImage;
    }

    public void ChangeAvailableAmount(int amount)
    {
        if (amount > 0)
        {
            gameObject.SetActive(true);
            if (amountText)
                amountText.text = amount.ToString();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
