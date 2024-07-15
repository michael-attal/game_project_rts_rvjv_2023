using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class MergeScreenPresenter : MonoBehaviour
{
    [SerializeField] private RectTransform container;
    [SerializeField] private FusionItemPresenter itemPresenter;
    [SerializeField] private SlimeMergeGraph mergeGraph;

    private List<FusionItemPresenter> items = new List<FusionItemPresenter>();
    
    // Start is called before the first frame update
    void Start()
    {
        foreach (var recipe in mergeGraph.GetRecipes())
        {
            var newItem = Instantiate(itemPresenter, container);
            newItem.Present(recipe.fusionInfo);
            items.Add(newItem);
        }
    }

    private void Update()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var gameEntity = entityManager.CreateEntityQuery(ComponentType.ReadOnly<Game>()).GetSingletonEntity();
        var currentFusionInfo = entityManager.GetComponentData<SlimeBasicUnitMerge>(gameEntity).FusionInfo;

        foreach (var item in items)
        {
            var tempFusionInfo = currentFusionInfo;
            int possibleAmount = 0;
            while (item.CurrentFusionInfo <= tempFusionInfo)
            {
                tempFusionInfo -= item.CurrentFusionInfo;
                ++possibleAmount;
            }
            item.ChangeAvailableAmount(possibleAmount);
        }
    }
}
