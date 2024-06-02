using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeScreenPresenter : MonoBehaviour
{
    [SerializeField] private UpgradeGraph _upgradeGraph;
    [SerializeField] private Transform _availableUpgradesParent;
    [SerializeField] private UpgradeItemPresenter _description;
    
    [SerializeField] private UpgradeItemPresenter _upgradePrefab;
    
    // Start is called before the first frame update
    void Start()
    {
        foreach (var upgrade in _upgradeGraph.upgrades)
        {
            var upgradeItem = Instantiate(_upgradePrefab, _availableUpgradesParent);
            upgradeItem.Present(upgrade);
            upgradeItem.Button.onClick.AddListener(() => _description.Present(upgrade));
        }
    }
}
