using TMPro;
using Unity.Entities;
using UnityEngine;

public class RessourceCountPresenter : MonoBehaviour
{
    [SerializeField] private TMP_Text counter;

    // Cache these to save time
    private EntityManager entityManager;
    private EntityQuery query;
    private Entity gameSingletonEntity;

    private void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        query = entityManager.CreateEntityQuery(typeof(Game));
    }

    void Update()
    {
        // The subscene takes a while to load so we have to wait until the singleton is found
        if (gameSingletonEntity != Entity.Null)
        {
            var currentRessources = entityManager.GetComponentData<Game>(gameSingletonEntity).RessourceCount;
            counter.text = $"{currentRessources}";
        }
        else
            UpdateEntityReferences();
    }

    private void UpdateEntityReferences()
    {
        if (!query.IsEmpty)
            gameSingletonEntity = query.GetSingletonEntity();
    }
}
