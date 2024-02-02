using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct UnitMovementSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Unit>();
        state.RequireForUpdate<UnitMovement>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Implement the shared movement system here.
        // If the movement system differs significantly between units, we should implement a specialized system, such as MySlimeUnitMovementSystem, instead of a generic one like this one.
    }
}