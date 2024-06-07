using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeScreenPresenter : MonoBehaviour
{
    [SerializeField] private UpgradeGraph _upgradeGraph;
    [SerializeField] private Transform _availableUpgradesParent;

    [SerializeField] private UpgradeItemPresenter _description;

    [SerializeField] private UpgradeItemPresenter _upgradePrefab;

    [Header("UpgradeSlots")] 
    [SerializeField] private UpgradeItemPresenter slot1;
    [SerializeField] private UpgradeItemPresenter slot2;
    [SerializeField] private UpgradeItemPresenter slot3;

    [Header("Buttons")] 
    [SerializeField] private Button saveButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button upgradeButton;

    private List<UpgradeDescriptor> _selectedUpgrades = new List<UpgradeDescriptor>();
    
    // Start is called before the first frame update
    void Start()
    {
        resetButton.onClick.AddListener(() => ResetUpgrades());
        upgradeButton.onClick.AddListener(() => ApplyUpgrades());
        
        foreach (var upgrade in _upgradeGraph.upgrades)
        {
            Debug.Log("Test");
            var upgradeItem = Instantiate(_upgradePrefab, _availableUpgradesParent);
            upgradeItem.Present(upgrade);
            upgradeItem.Button.onClick.AddListener(() =>
            {
                _description.Present(upgrade);
                saveButton.onClick.RemoveAllListeners();
                saveButton.onClick.AddListener(() => AddUpgrade(upgrade));
            });
        }
    }

    private void ResetUpgrades()
    {
        _selectedUpgrades.Clear();
        UpdateSelectedUpgradeSlots();
    }

    private void AddUpgrade(UpgradeDescriptor newUpgrade)
    {
        if (_selectedUpgrades.Contains(newUpgrade))
            return;
        
        _selectedUpgrades.Add(newUpgrade);
        
        if (_selectedUpgrades.Count > 3)
            _selectedUpgrades.RemoveAt(0);
        
        UpdateSelectedUpgradeSlots();
    }

    private void UpdateSelectedUpgradeSlots()
    {
        slot1.Clear();
        slot2.Clear();
        slot3.Clear();
        
        int nbUpgrades = _selectedUpgrades.Count;
        
        if (nbUpgrades >= 1)
            slot1.Present(_selectedUpgrades[0]);
        if (nbUpgrades >= 2)
            slot2.Present(_selectedUpgrades[1]);
        if (nbUpgrades == 3)
            slot3.Present(_selectedUpgrades[2]);
    }

    private void ApplyUpgrades()
    {
        var upgradeComponent = new SpawnerUpgradesRegister();
        foreach (var upgrade in _selectedUpgrades)
        {
            switch (upgrade.upgrade)
            {
                case MecaUpgrade.GLASS_CANNON:
                    upgradeComponent.HasGlassCannon = true;
                    break;
            }
        }

        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var gameEntity = entityManager.CreateEntityQuery(ComponentType.ReadOnly<Game>()).GetSingletonEntity();
        entityManager.AddComponentData(gameEntity, upgradeComponent);
    }
}
