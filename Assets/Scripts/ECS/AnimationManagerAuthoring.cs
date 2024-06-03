using Unity.Entities;
using UnityEngine;

public class AnimationManagerAuthoring : MonoBehaviour
{
    private class Baker : Baker<SpawnManagerAuthoring>
    {
        public override void Bake(SpawnManagerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent<AnimationManager>(entity);
        }
    }
}

public struct AnimationManager : IComponentData
{
    // NOTE: Not used anymore and not Burst Compile compatible
    // public static FixedString32Bytes GetAnimationNameFromAnimationsType(AnimationsType animationType)
    // {
    //     switch (animationType)
    //     {
    //         case AnimationsType.Idle:
    //             return new FixedString32Bytes("Idle");
    //
    //         case AnimationsType.Attack:
    //             return new FixedString32Bytes("Attack");
    //
    //         default:
    //             return new FixedString32Bytes("Idle");
    //     }
    // }
}


// NOTE: It's important that each unit prefab has the same order for the animations
public enum AnimationsType
{
    Idle = 0,
    Attack = 1,
    Move = 2
}