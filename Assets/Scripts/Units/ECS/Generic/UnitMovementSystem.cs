using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(TransformSystemGroup))]
// [UpdateAfter(typeof(UnitSelectableSystem))] // NOTE: We need to update after the to not lose the unit selection on the right click
public partial struct UnitMovementSystem : ISystem
{
    private bool isClicked;
    private float3 worldClickPosition;
    private EntityQuery isMovingTagQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<UnitSelectable>();
        state.RequireForUpdate<UnitMovement>();

        isMovingTagQuery = state.GetEntityQuery(ComponentType.ReadOnly<IsMovingTag>());
    }

    public void OnUpdate(ref SystemState state)
    {
        // Implement the shared movement system here.
        // If the movement system differs significantly between units, we should implement a specialized system, such as MySlimeUnitMovementSystem, in addition of a generic one like this one.

        var configManager = SystemAPI.GetSingleton<Config>();

        if (!configManager.ActivateUnitMovementSystem)
        {
            state.Enabled = false;
            return;
        }

        // NOTE: If you want some fun, you can make active unit follow the mouse on the ConfigManager :D
        if (configManager.ActivateUnitFollowMousePosition)
        {
            isClicked = true;
            var clickPos = Input.mousePosition;
            var mainCamera = Camera.main;
            if (mainCamera != null) worldClickPosition = mainCamera.ScreenToWorldPoint(new Vector3(clickPos.x, clickPos.y, mainCamera.transform.position.y));
        }
        else
        {
            // Check if there's a left click
            if (Input.GetMouseButtonDown(1))
            {
                isClicked = true;
                var clickPos = Input.mousePosition;
                var mainCamera = Camera.main;
                if (mainCamera != null) worldClickPosition = mainCamera.ScreenToWorldPoint(new Vector3(clickPos.x, clickPos.y, mainCamera.transform.position.y));
            }
        }

        // Check if any unit is moving
        var isUnitsMoving = isMovingTagQuery.IsEmptyIgnoreFilter;

        // If no units are moving and there's no left click, don't do anything in this frame
        if (isUnitsMoving == false && isClicked == false)
        {
            return;
        }

        var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        // TODO: S'assurer que la position n'est envoyée qu'une seule fois et qu'elle ne change pas pour les unités qui ne sont plus sélectionnées mais qui sont en mouvement. 
        // TODO: Donner la destination après avoir sélectionné des unités et effectué un clic droit et empecher de reprendre la précédente destination.
        var unitMovementJob = new UnitMovementJob
        {
            ECB = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
            DeltaTime = SystemAPI.Time.DeltaTime,
            Destination = new float3 { x = worldClickPosition.x, y = worldClickPosition.y, z = worldClickPosition.z }
        };

        unitMovementJob.ScheduleParallel();

        Debug.Log("Unit selected moved!");
    }
}

[WithAll(typeof(UnitMovement), typeof(UnitSelectable))]
[BurstCompile]
public partial struct UnitMovementJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ECB;
    public float DeltaTime;
    public float3 Destination;

    private void Execute(Entity entity, RefRO<UnitSelectable> unitSelectable, RefRW<UnitMovement> unitMovement, RefRW<LocalTransform> transform, [ChunkIndexInQuery] int chunkIndex)
    {
        if (unitSelectable.ValueRO.IsSelected == false)
            return;

        var direction = math.normalize(Destination - transform.ValueRO.Position);

        var gravity = new float3(0.0f, -9.82f, 0.0f);

        // Only update the destination if the unit is not already moving
        if (!unitMovement.ValueRW.IsMoving)
        {
            unitMovement.ValueRW.Destination = Destination;
        }

        var distanceToDestination = math.distance(transform.ValueRO.Position, Destination);
        if (distanceToDestination < 0.1f)
        {
            unitMovement.ValueRW.IsMoving = false;
            unitMovement.ValueRW.Velocity = 0;
            ECB.RemoveComponent<IsMovingTag>(chunkIndex, entity);
        }
        else
        {
            unitMovement.ValueRW.IsMoving = true;
            transform.ValueRW.Position += direction * unitMovement.ValueRO.Speed * DeltaTime;
            unitMovement.ValueRW.Velocity += gravity * DeltaTime;
            ECB.AddComponent<IsMovingTag>(chunkIndex, entity);
        }

        var speed = math.lengthsq(unitMovement.ValueRO.Velocity);
        if (speed < 0.1f)
        {
            unitMovement.ValueRW.Velocity = 0;
        }
    }
}