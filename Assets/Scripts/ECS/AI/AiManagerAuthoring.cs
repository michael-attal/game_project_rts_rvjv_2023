using Unity.Entities;
using UnityEngine;
using UnityEngine.Scripting;

// NOTE: Really simple AI (just plain algorithm for the moment) and no adjustment based on the player
// Put 50% of units to resource until it has 15 units that look for resources, the others are attacking the players.
// From the others, 80% will attack the closer unit and 20% others will attack (move) to the close building.
// Every time a resource seeker gets enough resources to build a base spawner it builds it.
// For the meca and slime, we randomly fuse or upgrade units.
public class AiManagerAuthoring : MonoBehaviour
{
    private class Baker : Baker<AiManagerAuthoring>
    {
        public override void Bake(AiManagerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);


            AddComponent(entity, new AI());
        }
    }
}


public struct AI : IComponentData
{
}

[WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
public partial class AISystemGroup : ComponentSystemGroup
{
    [Preserve]
    public AISystemGroup()
    {
    }
}