using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct UnitAttackSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Unit>();
        state.RequireForUpdate<UnitAttack>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Implement the shared attack system here.
        // If the Attack system differs significantly between units, we should implement a specialized system, such as MySlimeUnitAttackSystem, in addition of a generic one like this one.
    }
}