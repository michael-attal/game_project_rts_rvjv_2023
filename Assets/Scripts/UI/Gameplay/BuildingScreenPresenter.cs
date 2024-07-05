using UnityEngine;
using UnityEngine.UI;

public class BuildingScreenPresenter : MonoBehaviour
{
    [SerializeField] private Camera renderCamera;
    [SerializeField] private BuildingOptionsDescriptor buildings;
    [SerializeField] private RectTransform layout;
    [SerializeField] private BuildingItemPresenter buildingItem;

    private string selectedPrefab;
    public int SelectedPrefabID => selectedPrefab == "" ? 0 : selectedPrefab.GetHashCode();

    // Start is called before the first frame update
    private void Start()
    {
        Initialize(buildings);
    }

    public void ResetSelection()
    {
        selectedPrefab = "";
    }

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
            var shouldDisplayItem = true;

            var gameManagerGameObject = GameObject.Find("GameManager");

            if (gameManagerGameObject != null)
            {
                var gameManagerFromMonobehaviour = gameManagerGameObject.GetComponent<GameManager>();
                if (gameManagerFromMonobehaviour.GetSpeciesToPlay() == SpeciesToPlay.Slime)
                {
                    shouldDisplayItem = buildingData.title.Contains("Slime");
                    Debug.Log("Cacher le meca!");
                }

                if (gameManagerFromMonobehaviour.GetSpeciesToPlay() == SpeciesToPlay.Meca)
                {
                    shouldDisplayItem = buildingData.title.Contains("Meca");
                    Debug.Log("Cacher le slime!");
                }
            }

            if (shouldDisplayItem)
            {
                var newItem = Instantiate(buildingItem, layout);
                newItem.Initialize(buildingData);
                newItem.Button.onClick.AddListener(() => selectedPrefab = buildingData.id);
            }
        }
    }
}