using Unity.Entities;
using UnityEngine;

public class ExecuteSpawnManagerAuthoring : MonoBehaviour
{
    [Header("General Activation Settings")]
    //
    public bool ActiveGame;


    private class Baker : Baker<ExecuteSpawnManagerAuthoring>
    {
        public override void Bake(ExecuteSpawnManagerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            if (authoring.ActiveGame) AddComponent<BaseSpawnerBuilding>(entity);
        }
    }
}