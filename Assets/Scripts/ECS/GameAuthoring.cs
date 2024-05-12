using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class GameAuthoring : MonoBehaviour
{
    [SerializeField] private List<InstantiatableEntity> instantiatableEntities;
    
    private class Baker : Baker<GameAuthoring>
    {
        public override void Bake(GameAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new Game()
            {
                State = GameState.Starting,
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
    }

    
}

public struct Game : IComponentData
{
    public GameState State;
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