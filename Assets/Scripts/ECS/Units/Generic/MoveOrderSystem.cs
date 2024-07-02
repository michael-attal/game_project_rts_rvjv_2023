using AnimCooker;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(MouseSystemGroup))] // NOTE: We need to know if a mouse event occurred before updating this system
[UpdateBefore(typeof(MovementVelocity))]
[UpdateBefore(typeof(MovementPositionMotor))]
[UpdateBefore(typeof(MovementManualSystem))]
internal partial struct MoveOrderSystem : ISystem
{
    private EntityQuery manualMovementQuery;
    private EntityQuery velocityMovementQuery;
    private EntityQuery positionMotorMovementQuery;


    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<Game>();
        state.RequireForUpdate<MouseManager>();
        state.RequireForUpdate<MouseRightClickEvent>();

        manualMovementQuery = state.GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(MovementManual), typeof(LocalTransform) },
            Any = new ComponentType[] { typeof(Selected), typeof(UnitSelected), typeof(BuildingSelected) }
        });

        velocityMovementQuery = state.GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(MovementVelocity), typeof(LocalTransform) },
            Any = new ComponentType[] { typeof(Selected), typeof(UnitSelected), typeof(BuildingSelected) }
        });

        positionMotorMovementQuery = state.GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(MovementPositionMotor), typeof(LocalTransform) },
            Any = new ComponentType[] { typeof(Selected), typeof(UnitSelected), typeof(BuildingSelected) }
        });
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();
        var gameManager = SystemAPI.GetSingleton<Game>();

        if (!configManager.ActivateMoveOrderSystem)
        {
            state.Enabled = false;
            return;
        }

        if (gameManager.State == GameState.Paused)
            return;

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var mouseManagerEntity = SystemAPI.GetSingletonEntity<MouseManager>();
        var mouseManager = state.EntityManager.GetComponentData<MouseManager>(mouseManagerEntity);

        if (mouseManager.IsRightClickUp)
        {
            var mouseRightClickEventData = state.EntityManager.GetComponentData<MouseRightClickEvent>(mouseManagerEntity);

            // NOTE: Update ComponentLookups
            var manualMovementLookup = state.GetComponentLookup<MovementManual>(true);
            var velocityMovementLookup = state.GetComponentLookup<MovementVelocity>(true);
            var positionMotorMovementLookup = state.GetComponentLookup<MovementPositionMotor>(true);

            // NOTE: Process entities with manual movement
            ProcessManualMovementOrder(ref state, ref ecb, mouseRightClickEventData.PositionWorld, configManager.MovementFormationType, manualMovementQuery, manualMovementLookup);

            // NOTE: Process entities with standard MovementVelocity
            ProcessVelocityMovementOrder(ref state, ref ecb, mouseRightClickEventData.PositionWorld, configManager.MovementFormationType, velocityMovementQuery, velocityMovementLookup);

            // NOTE: Process entities with MovementPositionMotor
            ProcessPositionMotorMovementOrder(ref state, ref ecb, mouseRightClickEventData.PositionWorld, configManager.MovementFormationType, positionMotorMovementQuery, positionMotorMovementLookup);

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    [BurstCompile]
    private void ProcessManualMovementOrder(ref SystemState state, ref EntityCommandBuffer ecb, float3 targetPosition, FormationType formationType, EntityQuery query, ComponentLookup<MovementManual> componentLookup)
    {
        var count = query.CalculateEntityCount();

        if (count > 0)
        {
            var destinations = new NativeArray<float3>(count, Allocator.Temp);
            var index = 0;

            using (var entityArray = query.ToEntityArray(Allocator.TempJob))
            using (var transformArray = query.ToComponentDataArray<LocalTransform>(Allocator.TempJob))
            {
                for (var i = 0; i < entityArray.Length; i++)
                {
                    destinations[index] = CalculateFormationDestination(formationType, float3.zero, count, index, transformArray[i].Scale * 2f);
                    index++;
                }

                var centroid = float3.zero;
                for (var i = 0; i < count; i++)
                {
                    centroid += destinations[i];
                }

                centroid /= count;

                for (var i = 0; i < count; i++)
                {
                    destinations[i] = targetPosition + (destinations[i] - centroid);
                }

                index = 0;
                for (var i = 0; i < entityArray.Length; i++)
                {
                    var movementManual = componentLookup[entityArray[i]];
                    HandleMovement(ecb, entityArray[i], destinations[index], state.EntityManager.HasComponent<Unit>(entityArray[i]), movementManual.Speed, movementManual.IsMovementAnimated);
                    index++;
                }
            }

            destinations.Dispose();
        }
    }

    [BurstCompile]
    private void ProcessVelocityMovementOrder(ref SystemState state, ref EntityCommandBuffer ecb, float3 targetPosition, FormationType formationType, EntityQuery query, ComponentLookup<MovementVelocity> componentLookup)
    {
        var count = query.CalculateEntityCount();

        if (count > 0)
        {
            var destinations = new NativeArray<float3>(count, Allocator.Temp);
            var index = 0;

            using (var entityArray = query.ToEntityArray(Allocator.TempJob))
            using (var transformArray = query.ToComponentDataArray<LocalTransform>(Allocator.TempJob))
            {
                for (var i = 0; i < entityArray.Length; i++)
                {
                    destinations[index] = CalculateFormationDestination(formationType, float3.zero, count, index, transformArray[i].Scale * 2f);
                    index++;
                }

                var centroid = float3.zero;
                for (var i = 0; i < count; i++)
                {
                    centroid += destinations[i];
                }

                centroid /= count;

                for (var i = 0; i < count; i++)
                {
                    destinations[i] = targetPosition + (destinations[i] - centroid);
                }

                index = 0;
                for (var i = 0; i < entityArray.Length; i++)
                {
                    var movementVelocity = componentLookup[entityArray[i]];
                    HandleMovement(ecb, entityArray[i], destinations[index], state.EntityManager.HasComponent<Unit>(entityArray[i]), movementVelocity.Speed, movementVelocity.IsMovementAnimated);
                    index++;
                }
            }

            destinations.Dispose();
        }
    }

    [BurstCompile]
    private void ProcessPositionMotorMovementOrder(ref SystemState state, ref EntityCommandBuffer ecb, float3 targetPosition, FormationType formationType, EntityQuery query, ComponentLookup<MovementPositionMotor> componentLookup)
    {
        var count = query.CalculateEntityCount();

        if (count > 0)
        {
            var destinations = new NativeArray<float3>(count, Allocator.Temp);
            var index = 0;

            using (var entityArray = query.ToEntityArray(Allocator.TempJob))
            using (var transformArray = query.ToComponentDataArray<LocalTransform>(Allocator.TempJob))
            {
                for (var i = 0; i < entityArray.Length; i++)
                {
                    destinations[index] = CalculateFormationDestination(formationType, float3.zero, count, index, transformArray[i].Scale * 2f);
                    index++;
                }

                var centroid = float3.zero;
                for (var i = 0; i < count; i++)
                {
                    centroid += destinations[i];
                }

                centroid /= count;

                for (var i = 0; i < count; i++)
                {
                    destinations[i] = targetPosition + (destinations[i] - centroid);
                }

                index = 0;
                for (var i = 0; i < entityArray.Length; i++)
                {
                    var movementPositionMotor = componentLookup[entityArray[i]];
                    HandleMovement(ecb, entityArray[i], destinations[index], state.EntityManager.HasComponent<Unit>(entityArray[i]), movementPositionMotor.Speed, movementPositionMotor.IsMovementAnimated);
                    index++;
                }
            }

            destinations.Dispose();
        }
    }

    [BurstCompile]
    private float3 CalculateFormationDestination(FormationType formation, float3 basePosition, int count, int index, float spacing)
    {
        switch (formation)
        {
            case FormationType.Square:
                var side = (int)math.ceil(math.sqrt(count));
                var row = index / side;
                var col = index % side;
                return basePosition + new float3(col * spacing, 0, row * spacing);

            case FormationType.Circle:
                var angle = index / (float)count * 2 * math.PI;
                var radius = math.sqrt(count);
                return basePosition + new float3(math.cos(angle) * radius, 0, math.sin(angle) * radius);

            case FormationType.Scatter:
                var scatterRandom = new Random((uint)(index + 1));
                return basePosition + scatterRandom.NextFloat3Direction() * math.sqrt(count);

            default:
                return basePosition;
        }
    }

    [BurstCompile]
    private void HandleMovement(EntityCommandBuffer ecb, Entity entity, float3 destination, bool isUnit, float speed, bool isMovementAnimated)
    {
        ecb.SetComponentEnabled<WantsToMove>(entity, true);
        ecb.SetComponent(entity, new WantsToMove
        {
            Destination = destination
        });

        if (isUnit)
            ecb.SetComponentEnabled<UnitInMovementTag>(entity, true);

        if (isMovementAnimated)
        {
            // NOTE: Start move animation
            ecb.SetComponent(entity, new AnimationCmdData
            {
                Cmd = AnimationCmd.SetPlayForever, ClipIndex = (short)AnimationsType.Move
            });
            ecb.SetComponent(entity, new AnimationSpeedData
            {
                PlaySpeed = speed
            });
        }
    }
}

public enum FormationType
{
    Square,
    Circle,
    Scatter,
    None
}