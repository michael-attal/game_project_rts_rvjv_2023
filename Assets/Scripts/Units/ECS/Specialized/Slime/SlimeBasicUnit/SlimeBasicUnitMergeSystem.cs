using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct SlimeBasicUnitMergeSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SlimeBasicUnitMerge>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Implement the slime basic unit merge system here.
    }
}