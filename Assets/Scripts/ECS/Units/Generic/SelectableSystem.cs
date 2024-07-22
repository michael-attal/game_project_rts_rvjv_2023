using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(TransformSystemGroup))]
[UpdateAfter(typeof(MouseSystemGroup))]
[UpdateAfter(typeof(CameraManagerSystem))]
[BurstCompile]
public partial struct SelectableSystem : ISystem
{
    private const float minimumSelectionArea = 14f;
    private const float minimumSelectionAreaCenter = minimumSelectionArea / 2f;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<Game>();
        state.RequireForUpdate<Selectable>();
        state.RequireForUpdate<MouseManager>();
        state.RequireForUpdate<CameraManager>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();
        var gameManager = SystemAPI.GetSingleton<Game>();

        if (!configManager.ActivateSelectableSystem)
        {
            state.Enabled = false;
            return;
        }

        if (gameManager.State == GameState.Paused)
            return;

        var mouseManager = SystemAPI.GetSingleton<MouseManager>();

        if (mouseManager.IsLeftClickUp)
        {
            if (mouseManager.IgnoreNextClick)
            {
                Debug.Log("Ignoring click");
                mouseManager.IgnoreNextClick = false;
                SystemAPI.SetSingleton(mouseManager);
                return;
            }

            var cameraManager = SystemAPI.GetSingleton<CameraManager>();

            var initialClickPosition = mouseManager.InitialClickPosition;
            var finalClickPosition = mouseManager.FinalClickPosition;

            var left = Mathf.Min(initialClickPosition.x, finalClickPosition.x);
            var top = Mathf.Min(initialClickPosition.y, finalClickPosition.y);
            var width = Mathf.Abs(initialClickPosition.x - finalClickPosition.x);
            var height = Mathf.Abs(initialClickPosition.y - finalClickPosition.y);

            // NOTE: Incorporate a slight radius to enable entity selection with a single click.
            var selectionArea = new Rect(left - minimumSelectionAreaCenter, top - minimumSelectionAreaCenter,
                width + minimumSelectionArea, height + minimumSelectionArea);

            var selectionJob = new SelectionJob
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
                SelectionArea = selectionArea,
                CurrentPlayerSpecies = gameManager.SpeciesToPlay
            };
            selectionJob.ScheduleParallel();
        }
    }
}

[WithAll(typeof(Selectable))]
[BurstCompile]
public partial struct SelectionJob : IJobEntity
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
    public SpeciesToPlay CurrentPlayerSpecies;

    // NOTE: Because we want the global position of a child entity, we read LocalToWorld instead of LocalTransform.
    private void Execute(Entity entity, LocalToWorld entityLT, SpeciesTag species, [ChunkIndexInQuery] int chunkIndex)
    {
        if (GameManager.IsControlledByCurrentPlayer(CurrentPlayerSpecies, species.Type))
        {
            var entityRadius = entityLT.Value.Scale().x;

            var transformScreenPosition = CameraManagerTools.ConvertWorldToScreenCoordinates(
                entityLT.Position,
                CameraPos,
                CamProjMatrix,
                CamUp,
                CamRight,
                CamForward,
                PixelWidth,
                PixelHeight,
                ScaleFactor // or entityRadius ?
            );

            // NOTE: Add the entity radius to the selection
            var entityRect = new Rect(transformScreenPosition.x - entityRadius, transformScreenPosition.y - entityRadius,
                entityRadius * 2, entityRadius * 2);

            // NOTE: Check if selection intersect with the entity
            if (entityRect.Overlaps(SelectionArea, true))
            {
                ECB.SetComponentEnabled<Selected>(chunkIndex, entity, true);
            }
            else
            {
                ECB.SetComponentEnabled<Selected>(chunkIndex, entity, false);
            }
        }
    }
}