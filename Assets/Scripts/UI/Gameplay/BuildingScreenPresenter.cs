using UnityEngine;
using UnityEngine.UI;

public class BuildingScreenPresenter : MonoBehaviour
{
    public int SelectedPrefabID => selectedPrefab == "" ? 0 : selectedPrefab.GetHashCode();
    
    [SerializeField] private Camera renderCamera;
    [SerializeField] private BuildingOptionsDescriptor buildings;
    [SerializeField] private RectTransform layout;
    [SerializeField] private BuildingItemPresenter buildingItem;

    private string selectedPrefab;

    // Start is called before the first frame update
    void Start()
    {
        Initialize(buildings);
    }

    public void ResetSelection()
        => selectedPrefab = "";

    private void Initialize(BuildingOptionsDescriptor buildings)
    {
        selectedPrefab = "";
        
        // Clear all items
        while (layout.childCount > 0)
        {
            var child = layout.GetChild(0);
            if (child.TryGetComponent(out Button button))
                button.onClick.RemoveAllListeners();
            
            Destroy(child);
        }
        
        // Instantiate new items
        foreach (var buildingData in buildings.prefabsIdList)
        {
            var newItem = Instantiate(buildingItem, layout);
            newItem.Initialize(buildingData);
            newItem.Button.onClick.AddListener(() => selectedPrefab = buildingData.id);
        }
    }
}
