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
    private int lastClickID;

    private EntityQuery manualMovementQuery;
    private EntityQuery velocityMovementQuery;
    private EntityQuery positionMotorMovementQuery;


    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<MouseManager>();
        state.RequireForUpdate<MouseRightClickEvent>();

        lastClickID = -1;

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

        if (!configManager.ActivateMoveOrderSystem)
        {
            state.Enabled = false;
            return;
        }

        if (configManager.IsGamePaused)
            return;

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var mouseManagerEntity = SystemAPI.GetSingletonEntity<MouseManager>();

        var mouseRightClickEventData = state.EntityManager.GetComponentData<MouseRightClickEvent>(mouseManagerEntity);

        var isNewClickEventDetected = lastClickID != mouseRightClickEventData.RightClickID;

        if (!isNewClickEventDetected)
            return;

        // NOTE: Update ComponentLookups
        var manualMovementLookup = state.GetComponentLookup<MovementManual>(true);
        var velocityMovementLookup = state.GetComponentLookup<MovementVelocity>(true);
        var positionMotorMovementLookup = state.GetComponentLookup<MovementPositionMotor>(true);

        // NOTE: Process entities with manual movement
        ProcessManualMovementOrder(ref state, ref ecb, mouseRightClickEventData.Position, configManager.MovementFormationType, manualMovementQuery, manualMovementLookup);

        // NOTE: Process entities with standard MovementVelocity
        ProcessVelocityMovementOrder(ref state, ref ecb, mouseRightClickEventData.Position, configManager.MovementFormationType, velocityMovementQuery, velocityMovementLookup);

        // NOTE: Process entities with MovementPositionMotor
        ProcessPositionMotorMovementOrder(ref state, ref ecb, mouseRightClickEventData.Position, configManager.MovementFormationType, positionMotorMovementQuery, positionMotorMovementLookup);

        lastClickID = mouseRightClickEventData.RightClickID;
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
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

// NOTE: Backup code with too much duplicated code
//
// using AnimCooker;
// using Unity.Burst;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Mathematics;
// using Unity.Transforms;
//
// [UpdateAfter(typeof(MouseSystemGroup))] // NOTE: We need to know if a mouse event occurred before updating this system
// [UpdateBefore(typeof(MovementVelocity))]
// [UpdateBefore(typeof(MovementPositionMotor))]
// [UpdateBefore(typeof(MovementManualSystem))]
// internal partial struct MoveOrderSystem : ISystem
// {
//     private int lastClickID;
//
//     [BurstCompile]
//     public void OnCreate(ref SystemState state)
//     {
//         state.RequireForUpdate<Config>();
//         state.RequireForUpdate<MouseManager>();
//         state.RequireForUpdate<MouseRightClickEvent>();
//
//         lastClickID = -1;
//     }
//
//     [BurstCompile]
//     public void OnUpdate(ref SystemState state)
//     {
//         var configManager = SystemAPI.GetSingleton<Config>();
//
//         if (!configManager.ActivateMoveOrderSystem)
//         {
//             state.Enabled = false;
//             return;
//         }
//
//         if (configManager.IsGamePaused)
//             return;
//
//         var ecb = new EntityCommandBuffer(Allocator.Temp);
//
//         var mouseManagerEntity = SystemAPI.GetSingletonEntity<MouseManager>();
//
//         var mouseRightClickEventData = state.EntityManager.GetComponentData<MouseRightClickEvent>(mouseManagerEntity);
//
//         var isNewClickEventDetected = lastClickID != mouseRightClickEventData.RightClickID;
//
//         if (!isNewClickEventDetected)
//             return;
//
//         // NOTE: Process entities with manual movement
//         var query = SystemAPI.QueryBuilder().WithAll<MovementManual>().WithAny<Selected, UnitSelected, BuildingSelected>().Build();
//         var count = query.CalculateEntityCount();
//
//         if (count > 0)
//         {
//             // Step 1: Calculate relative destinations
//             var destinations = new NativeArray<float3>(count, Allocator.Temp);
//             var index = 0;
//             foreach (var (_, transform, entity) in SystemAPI
//                          .Query<RefRO<MovementManual>, RefRO<LocalTransform>>()
//                          .WithAny<Selected, UnitSelected, BuildingSelected>() // NOTE: In the case of allowing the movement of a building, such as in Warcraft 3 for elven buildings.
//                          .WithEntityAccess())
//             {
//                 destinations[index] = CalculateFormationDestination(configManager.MovementFormationType, float3.zero, count, index, transform.ValueRO.Scale * 2f);
//                 index++;
//             }
//
//             // Step 2: Calculate the centroid of the relative destinations
//             var centroid = float3.zero;
//             for (var i = 0; i < count; i++)
//             {
//                 centroid += destinations[i];
//             }
//
//             centroid /= count;
//
//             // Step 3: Offset destinations to center them around mouseRightClickEventData.Position
//             var targetPosition = mouseRightClickEventData.Position;
//             for (var i = 0; i < count; i++)
//             {
//                 destinations[i] = targetPosition + (destinations[i] - centroid);
//             }
//
//             // Step 4: Apply the calculated destinations
//             index = 0;
//             foreach (var (movementManual, transform, entity) in SystemAPI
//                          .Query<RefRO<MovementManual>, RefRO<LocalTransform>>()
//                          .WithAny<Selected, UnitSelected, BuildingSelected>()
//                          .WithEntityAccess())
//             {
//                 HandleMovement(ecb, entity, destinations[index], SystemAPI.HasComponent<Unit>(entity), movementManual.ValueRO.Speed, movementManual.ValueRO.IsMovementAnimated);
//                 index++;
//             }
//
//             destinations.Dispose();
//         }
//
//         // TODO: Maybe use something like ComponentLookup<MovementVelocity> MovementVelocityLookup; with a function to avoid repetitive code
//         //Process entities with standard MovementVelocity
//         query = SystemAPI.QueryBuilder().WithAll<MovementVelocity>().WithAny<Selected, UnitSelected, BuildingSelected>().Build();
//         count = query.CalculateEntityCount();
//         if (count > 0)
//         {
//             var destinations = new NativeArray<float3>(count, Allocator.Temp);
//             var index = 0;
//             foreach (var (_, transform, entity) in SystemAPI
//                          .Query<RefRO<MovementVelocity>, RefRO<LocalTransform>>()
//                          .WithAny<Selected, UnitSelected, BuildingSelected>()
//                          .WithEntityAccess())
//             {
//                 destinations[index] = CalculateFormationDestination(configManager.MovementFormationType, float3.zero, count, index, transform.ValueRO.Scale * 2f);
//                 index++;
//             }
//
//             var centroid = float3.zero;
//             for (var i = 0; i < count; i++)
//             {
//                 centroid += destinations[i];
//             }
//
//             centroid /= count;
//
//             var targetPosition = mouseRightClickEventData.Position;
//             for (var i = 0; i < count; i++)
//             {
//                 destinations[i] = targetPosition + (destinations[i] - centroid);
//             }
//
//             index = 0;
//             foreach (var (movementVelocity, transform, entity) in SystemAPI
//                          .Query<RefRO<MovementVelocity>, RefRO<LocalTransform>>()
//                          .WithAny<Selected, UnitSelected, BuildingSelected>()
//                          .WithEntityAccess())
//             {
//                 HandleMovement(ecb, entity, destinations[index], SystemAPI.HasComponent<Unit>(entity), movementVelocity.ValueRO.Speed, movementVelocity.ValueRO.IsMovementAnimated);
//                 index++;
//             }
//
//             destinations.Dispose();
//         }
//
//         // Process entities with MovementPositionMotor
//         query = SystemAPI.QueryBuilder().WithAll<MovementPositionMotor>().WithAny<Selected, UnitSelected, BuildingSelected>().Build();
//         count = query.CalculateEntityCount();
//         if (count > 0)
//         {
//             var destinations = new NativeArray<float3>(count, Allocator.Temp);
//             var index = 0;
//             foreach (var (_, transform, entity) in SystemAPI
//                          .Query<RefRO<MovementPositionMotor>, RefRO<LocalTransform>>()
//                          .WithAny<Selected, UnitSelected, BuildingSelected>()
//                          .WithEntityAccess())
//             {
//                 destinations[index] = CalculateFormationDestination(configManager.MovementFormationType, float3.zero, count, index, transform.ValueRO.Scale * 2f);
//                 index++;
//             }
//
//             var centroid = float3.zero;
//             for (var i = 0; i < count; i++)
//             {
//                 centroid += destinations[i];
//             }
//
//             centroid /= count;
//
//             var targetPosition = mouseRightClickEventData.Position;
//             for (var i = 0; i < count; i++)
//             {
//                 destinations[i] = targetPosition + (destinations[i] - centroid);
//             }
//
//             index = 0;
//             foreach (var (movementPositionMotor, transform, entity) in SystemAPI
//                          .Query<RefRO<MovementPositionMotor>, RefRO<LocalTransform>>()
//                          .WithAny<Selected, UnitSelected, BuildingSelected>()
//                          .WithEntityAccess())
//             {
//                 HandleMovement(ecb, entity, destinations[index], SystemAPI.HasComponent<Unit>(entity), movementPositionMotor.ValueRO.Speed, movementPositionMotor.ValueRO.IsMovementAnimated);
//                 index++;
//             }
//
//             destinations.Dispose();
//         }
//
//         lastClickID = mouseRightClickEventData.RightClickID;
//         ecb.Playback(state.EntityManager);
//         ecb.Dispose();
//     }
//
//     private float3 CalculateFormationDestination(FormationType formation, float3 basePosition, int count, int index, float spacing)
//     {
//         switch (formation)
//         {
//             case FormationType.Square:
//                 var side = (int)math.ceil(math.sqrt(count));
//                 var row = index / side;
//                 var col = index % side;
//                 return basePosition + new float3(col * spacing, 0, row * spacing);
//
//             case FormationType.Circle:
//                 var angle = index / (float)count * 2 * math.PI;
//                 var radius = math.sqrt(count);
//                 return basePosition + new float3(math.cos(angle) * radius, 0, math.sin(angle) * radius);
//
//             case FormationType.Scatter:
//                 var scatterRandom = new Random((uint)(index + 1));
//                 return basePosition + scatterRandom.NextFloat3Direction() * math.sqrt(count);
//
//             default:
//                 return basePosition;
//         }
//     }
//
//     [BurstCompile]
//     private void HandleMovement(EntityCommandBuffer ecb, Entity entity, float3 destination, bool isUnit, float speed, bool isMovementAnimated)
//     {
//         ecb.SetComponentEnabled<WantsToMove>(entity, true);
//         ecb.SetComponent(entity, new WantsToMove
//         {
//             Destination = destination
//         });
//
//         if (isUnit)
//             ecb.SetComponentEnabled<UnitInMovementTag>(entity, true);
//
//         if (isMovementAnimated)
//         {
//             // NOTE: Start move animation
//             ecb.SetComponent(entity, new AnimationCmdData
//             {
//                 Cmd = AnimationCmd.SetPlayForever, ClipIndex = (short)AnimationsType.Move
//             });
//             ecb.SetComponent(entity, new AnimationSpeedData
//             {
//                 PlaySpeed = speed
//             });
//         }
//     }
// }
//
// public enum FormationType
// {
//     Square,
//     Circle,
//     Scatter,
//     None
// }