using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeItemPresenter : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _description;
    [SerializeField] private Button _button;

    private UpgradeDescriptor _currentUpgrade;

    public UpgradeDescriptor Upgrade
        => _currentUpgrade;

    public Button Button
        => _button;
    
    public void Present(UpgradeDescriptor upgrade)
    {
        if (_image)
            _image.sprite = upgrade.image;

        if (_title)
            _title.text = upgrade.title;

        if (_description)
            _description.text = upgrade.description;
    }
}
