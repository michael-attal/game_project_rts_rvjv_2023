using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct UnitSelectableSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Unit>();
        state.RequireForUpdate<UnitSelectable>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Implement the shared unit selectable system here.
        // If the selectable system differs significantly between units, we should implement a specialized system, such as MySlimeUnitSelectableSystem, instead of a generic one like this one.
    }
}