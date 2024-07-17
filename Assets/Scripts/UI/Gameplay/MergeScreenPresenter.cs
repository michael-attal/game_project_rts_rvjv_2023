using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class MergeScreenPresenter : MonoBehaviour
{
    [SerializeField] private RectTransform container;
    [SerializeField] private FusionItemPresenter itemPresenter;
    [SerializeField] private SlimeMergeGraph mergeGraph;

    private List<FusionItemPresenter> items = new List<FusionItemPresenter>();
    private EntityQuery selectedEntitiesQuery;
    
    // Start is called before the first frame update
    void Start()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        selectedEntitiesQuery = new EntityQueryBuilder(Allocator.Persistent)
            .WithAll<Selected, SlimeBasicUnitMerge>()
            .WithPresent<WantsToMerge>()
            .Build(entityManager);
        
        foreach (var recipe in mergeGraph.GetRecipes())
        {
            var newItem = Instantiate(itemPresenter, container);
            newItem.Present(recipe.fusionInfo);
            newItem.Button.onClick.AddListener(() => OnButtonClick(recipe));
            items.Add(newItem);
        }
    }

    private void OnButtonClick(FusionRecipe recipe)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var gameEntity = entityManager.CreateEntityQuery(ComponentType.ReadOnly<Game>()).GetSingletonEntity();

        int amountOrdered = Input.GetKey(KeyCode.LeftShift) ? 5 : 1;
        
        entityManager.AddComponentData(gameEntity, new FusionOrder(amountOrdered, recipe.ToData()));

        foreach (var entity in selectedEntitiesQuery.ToEntityArray(Allocator.Temp))
        {
            entityManager.SetComponentEnabled<WantsToMerge>(entity, true);
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
