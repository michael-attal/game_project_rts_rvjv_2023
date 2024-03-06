using Unity.Entities;
using UnityEngine;

public class GameAuthoring : MonoBehaviour
{
    private class Baker : Baker<GameAuthoring>
    {
        public override void Bake(GameAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new Game()
            {
                State = GameState.Starting 
            });
        }
    }
}

public struct Game : IComponentData
{
    public GameState State;
}
