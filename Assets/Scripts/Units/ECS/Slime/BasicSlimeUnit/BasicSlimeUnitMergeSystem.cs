using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct BasicSlimeUnitMergeSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BasicSlimeUnit>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Implement the slime basic unit merge system here.
    }
}