using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class GameAuthoring : MonoBehaviour
{
    [SerializeField] private List<InstantiatableEntity> instantiatableEntities;
    [SerializeField] private SlimeMergeGraph mergeGraph;
    
    private class Baker : Baker<GameAuthoring>
    {
        public override void Bake(GameAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            var slimeRecipes = authoring.mergeGraph.GetRecipes();
            var slimeRecipesData = slimeRecipes
                .Select(recipe =>
                {
                    return new FusionRecipeData()
                    {
                        PrefabId = recipe.entityPrefab.GetHashCode(),
                        Cost = recipe.cost
                    };
                })
                .ToArray();
            
            AddComponent(entity, new Game()
            {
                State = GameState.Starting,
                SlimeRecipes = GetRecipeDataBlob(slimeRecipesData),
                RessourceCount = 10
            });

            DynamicBuffer<InstantiatableEntityData> buffer = AddBuffer<InstantiatableEntityData>(entity);
            buffer.Length = authoring.instantiatableEntities.Count;
            for (int i = 0; i < authoring.instantiatableEntities.Count; ++i)
            {
                InstantiatableEntity instantiatable = authoring.instantiatableEntities[i];
                buffer[i] = new InstantiatableEntityData
                {
                    EntityID = instantiatable.entityID.GetHashCode(),
                    Entity = GetEntity(instantiatable.entity, TransformUsageFlags.Dynamic)
                };
            }
        }

        private BlobAssetReference<FusionRecipeDataPool> GetRecipeDataBlob(IEnumerable<FusionRecipeData> recipeDatas)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref FusionRecipeDataPool recipeDataPool = ref builder.ConstructRoot<FusionRecipeDataPool>();

            var recipeDataArray = recipeDatas
                .OrderByDescending(recipeData => recipeData.Cost)
                .ToArray();
            
            var arrayBuilder = builder.Allocate(
                ref recipeDataPool.Data,
                recipeDataArray.Length
            );

            for (int i = 0; i < recipeDataArray.Length; ++i)
                arrayBuilder[i] = recipeDataArray[i];

            var result = builder.CreateBlobAssetReference<FusionRecipeDataPool>(Allocator.Persistent);
            builder.Dispose();
            return result;
        }
    }

    
}

public struct FusionRecipeDataPool
{
    public BlobArray<FusionRecipeData> Data;
}

public struct Game : IComponentData
{
    public BlobAssetReference<FusionRecipeDataPool> SlimeRecipes;
    public GameState State;
    public int RessourceCount;
}

[Serializable]
public struct InstantiatableEntity
{
    public string entityID;
    public GameObject entity;
}

public struct InstantiatableEntityData : IBufferElementData
{
    public int EntityID;
    public Entity Entity;
}