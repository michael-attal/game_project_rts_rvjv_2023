using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(TransformSystemGroup))]
// [UpdateAfter(typeof(UnitSelectableSystem))] // NOTE: We need to update after the to not lose the unit selection on the right click
public partial struct UnitMovementSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<UnitSelectable>();
        state.RequireForUpdate<UnitMovement>();
    }

    public void OnUpdate(ref SystemState state)
    {
        // Implement the shared movement system here.
        // If the movement system differs significantly between units, we should implement a specialized system, such as MySlimeUnitMovementSystem, in addition of a generic one like this one.

        if (!Input.GetMouseButtonDown(1))
            return;

        var clickPos = Input.mousePosition;
        var mainCamera = Camera.main;
        var clickWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(clickPos.x, clickPos.y, mainCamera.transform.position.y));


        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

        var unitMovementJob = new UnitMovementJob
        {
            ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged),
            DeltaTime = SystemAPI.Time.DeltaTime,
            Destination = new float3 { x = clickWorldPosition.x, y = clickWorldPosition.y, z = clickWorldPosition.z }
        };

        unitMovementJob.Schedule(); // TODO: Perhaps consider switching to .ScheduleParallel(); ? If we decide to do so, we will need to change "ref" in the Job to "RefRO<>" or "RefRW<>".

        // foreach (var (unitMovementLTW, unitSelectable)
        //          in
        //          SystemAPI
        //              .Query<RefRW<LocalToWorld>, RefRO<UnitSelectable>>()
        //              .WithAll<UnitMovement>())
        // {
        //     // TODO: Create a destination float3 data within the UnitMovement component, which will store the intended world position to travel to. Subsequently, assign a velocity to the selected units, and ultimately eliminate the velocity prior to arrival. 
        //     // TODO: Avoid clustering all units together by spacing them out at a fixed distance from each other.
        //     if (unitSelectable.ValueRO.IsSelected)
        //     {
        //         var newPosition = new float3(
        //             clickWorldPosition.x,
        //             clickWorldPosition.y,
        //             clickWorldPosition.z
        //         );
        //
        //         unitMovementLTW.ValueRW.Value =
        //             float4x4.TRS(newPosition, quaternion.identity, unitMovementLTW.ValueRO.Value.Scale());
        //     }
        // }

        Debug.Log("Unit selected moved!");
    }
}

[WithAll(typeof(UnitMovement), typeof(UnitSelectable))]
[BurstCompile]
public partial struct UnitMovementJob : IJobEntity
{
    public EntityCommandBuffer ECB;
    public float DeltaTime;
    public float3 Destination;

    private void Execute(Entity entity, RefRO<UnitSelectable> unitSelectable, ref UnitMovement unitMovement, ref LocalTransform transform)
    {
        if (!unitSelectable.ValueRO.IsSelected)
            return;

        transform.Position = Destination;

        // var gravity = new float3(0.0f, -9.82f, 0.0f);
        // var invertY = new float3(1.0f, -1.0f, 1.0f);
        // unitMovement.Destination = Destination;
        //
        // transform.Position += unitMovement.Velocity * DeltaTime;
        //
        // // bounce on the ground
        // if (transform.Position.y < 0.0f)
        // {
        //     transform.Position *= invertY;
        //     unitMovement.Velocity *= invertY * 0.8f;
        // }
        //
        // unitMovement.Velocity += gravity * DeltaTime;
        //
        // var speed = math.lengthsq(unitMovement.Velocity);
        // if (speed < 0.1f)
        // {
        //     unitMovement.Velocity = 0;
        // }
    }
}