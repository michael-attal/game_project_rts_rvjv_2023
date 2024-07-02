using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(TransformSystemGroup))]
[UpdateAfter(typeof(MouseSystemGroup))]
[UpdateAfter(typeof(CameraManagerSystem))]
[BurstCompile]
public partial struct UnitSelectableSystem : ISystem
{
    private const float minimumSelectionArea = 14f;
    private const float minimumSelectionAreaCenter = minimumSelectionArea / 2f;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<Game>();
        state.RequireForUpdate<UnitSelectable>();
        state.RequireForUpdate<MouseManager>();
        state.RequireForUpdate<CameraManager>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();
        var gameManager = SystemAPI.GetSingleton<Game>();

        if (!configManager.ActivateUnitSelectableSystem)
        {
            state.Enabled = false;
            return;
        }

        if (gameManager.State == GameState.Paused)
            return;

        var mouseManager = SystemAPI.GetSingleton<MouseManager>();

        if (mouseManager.IsLeftClickUp)
        {
            var cameraManager = SystemAPI.GetSingleton<CameraManager>();

            var initialClickPosition = mouseManager.InitialClickPosition;
            var finalClickPosition = mouseManager.FinalClickPosition;

            var left = Mathf.Min(initialClickPosition.x, finalClickPosition.x);
            var top = Mathf.Min(initialClickPosition.y, finalClickPosition.y);
            var width = Mathf.Abs(initialClickPosition.x - finalClickPosition.x);
            var height = Mathf.Abs(initialClickPosition.y - finalClickPosition.y);

            // NOTE: Incorporate a slight radius to enable unit selection with a single click.
            var selectionArea = new Rect(left - minimumSelectionAreaCenter, top - minimumSelectionAreaCenter,
                width + minimumSelectionArea, height + minimumSelectionArea);

            var unitSelectionJob = new UnitSelectionJob
            {
                ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                CameraPos = cameraManager.Position,
                CamProjMatrix = cameraManager.ProjectionMatrix,
                CamUp = cameraManager.Up,
                CamRight = cameraManager.Right,
                CamForward = cameraManager.Forward,
                PixelWidth = cameraManager.PixelWidth,
                PixelHeight = cameraManager.PixelHeight,
                ScaleFactor = cameraManager.ScaleFactor,
                SelectionArea = selectionArea
            };
            unitSelectionJob.ScheduleParallel();
        }
    }
}

[WithAll(typeof(UnitSelectable))]
[BurstCompile]
public partial struct UnitSelectionJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ECB;
    public float3 CameraPos;
    public float4x4 CamProjMatrix;
    public float3 CamUp;
    public float3 CamRight;
    public float3 CamForward;
    public float PixelWidth;
    public float PixelHeight;
    public float ScaleFactor;
    public Rect SelectionArea;

    // NOTE: Because we want the global position of a child entity, we read LocalToWorld instead of LocalTransform.
    private void Execute(Entity entity, LocalToWorld unitLT, [ChunkIndexInQuery] int chunkIndex)
    {
        var unitRadius = unitLT.Value.Scale().x;

        var transformScreenPosition = CameraManagerTools.ConvertWorldToScreenCoordinates(
            unitLT.Position,
            CameraPos,
            CamProjMatrix,
            CamUp,
            CamRight,
            CamForward,
            PixelWidth,
            PixelHeight,
            ScaleFactor // or unitRadius ?
        );

        // NOTE: Add the unit radius to the selection
        var unitRect = new Rect(transformScreenPosition.x - unitRadius, transformScreenPosition.y - unitRadius,
            unitRadius * 2, unitRadius * 2);

        // NOTE: Check if selection intersect with unit
        if (unitRect.Overlaps(SelectionArea, true))
        {
            ECB.SetComponentEnabled<UnitSelected>(chunkIndex, entity, true);
        }
        else
        {
            ECB.SetComponentEnabled<UnitSelected>(chunkIndex, entity, false);
        }
    }
}