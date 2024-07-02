using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Scripting;

public class MouseManagerAuthoring : MonoBehaviour
{
    [SerializeField] private bool activateRightClickEventSystem;
    [SerializeField] private bool activateLeftClickEventSystem;
    [SerializeField] private bool activateMiddleClickEventSystem;
    [SerializeField] private bool activateMouseMovementEventSystem;

    private class Baker : Baker<MouseManagerAuthoring>
    {
        public override void Bake(MouseManagerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new MouseManager
            {
                ActivateRightClickEventSystem = authoring.activateRightClickEventSystem,
                ActivateLeftClickEventSystem = authoring.activateLeftClickEventSystem,
                ActivateMiddleClickEventSystem = authoring.activateMiddleClickEventSystem,
                ActivateMouseMovementEventSystem = authoring.activateMouseMovementEventSystem,
                InitialClickPosition = float3.zero,
                FinalClickPosition = float3.zero,
                InitialClickPositionWorld = float3.zero,
                FinalClickPositionWorld = float3.zero,
                IsLeftClickDown = false,
                IsLeftClickUp = false,
                IsRightClickDown = false,
                IsRightClickUp = false,
                IsMiddleClickDown = false,
                IsMiddleClickUp = false
            });

            AddComponent(entity, new MouseLeftClickEvent { LastPosition = float3.zero, Position = float3.zero, LastPositionWorld = float3.zero, PositionWorld = float3.zero, LeftClickID = -1 });
            AddComponent(entity, new MouseRightClickEvent { LastPosition = float3.zero, Position = float3.zero, LastPositionWorld = float3.zero, PositionWorld = float3.zero, RightClickID = -1 });
            AddComponent(entity, new MouseMiddleClickEvent { LastPosition = float3.zero, Position = float3.zero, LastPositionWorld = float3.zero, PositionWorld = float3.zero, MiddleClickID = -1 });
            AddComponent(entity, new MouseMovementEvent { LastPosition = float3.zero, Position = float3.zero, LastPositionWorld = float3.zero, PositionWorld = float3.zero, MovementID = -1 });
        }
    }
}


[WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
public partial class MouseSystemGroup : ComponentSystemGroup
{
    [Preserve]
    public MouseSystemGroup()
    {
    }
}

[RequireMatchingQueriesForUpdate]
[WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
[UpdateInGroup(typeof(MouseSystemGroup))]
public partial struct MouseEventSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<Game>();
        state.RequireForUpdate<MouseManager>();
        state.RequireForUpdate<MouseRightClickEvent>();
        state.RequireForUpdate<MouseLeftClickEvent>();
        state.RequireForUpdate<MouseMiddleClickEvent>();
        state.RequireForUpdate<MouseMovementEvent>();
    }

    // [BurstCompile] Cannot burst compile: Access to camera
    public void OnUpdate(ref SystemState state)
    {
        var configManager = SystemAPI.GetSingleton<Config>();
        var gameManager = SystemAPI.GetSingleton<Game>();

        if (configManager.ActivateMouseManagerSystem == false)
        {
            state.Enabled = false;
            return;
        }

        if (gameManager.State == GameState.Paused)
            return;

        var mouseManagerEntity = SystemAPI.GetSingletonEntity<MouseManager>();
        var mouseManager = SystemAPI.GetComponent<MouseManager>(mouseManagerEntity);

        var mousePosition = Input.mousePosition;
        var mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("Error: Main camera doesn't exist");
            return;
        }

        var mouseWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, mainCamera.transform.position.y));

        // NOTE: Reset click-up states
        mouseManager.IsLeftClickUp = false;
        mouseManager.IsRightClickUp = false;
        mouseManager.IsMiddleClickUp = false;

        if (mouseManager.ActivateLeftClickEventSystem)
        {
            if (Input.GetMouseButtonDown(0) && !mouseManager.IsLeftClickDown)
            {
                mouseManager.IsLeftClickDown = true;
                mouseManager.InitialClickPosition = mousePosition;
                mouseManager.InitialClickPositionWorld = mouseWorldPosition;
            }
            else if (Input.GetMouseButtonUp(0) && mouseManager.IsLeftClickDown)
            {
                mouseManager.IsLeftClickDown = false;
                mouseManager.IsLeftClickUp = true;
                mouseManager.FinalClickPosition = mousePosition;
                mouseManager.FinalClickPositionWorld = mouseWorldPosition;
                var previousData = SystemAPI.GetComponent<MouseLeftClickEvent>(mouseManagerEntity);
                SystemAPI.SetComponent(mouseManagerEntity, new MouseLeftClickEvent
                {
                    LastPosition = previousData.Position,
                    Position = mousePosition,
                    LastPositionWorld = previousData.PositionWorld,
                    PositionWorld = mouseWorldPosition,
                    LeftClickID = ++previousData.LeftClickID
                });
            }
        }

        if (mouseManager.ActivateRightClickEventSystem)
        {
            if (Input.GetMouseButtonDown(1) && !mouseManager.IsRightClickDown)
            {
                mouseManager.IsRightClickDown = true;
            }
            else if (Input.GetMouseButtonUp(1) && mouseManager.IsRightClickDown)
            {
                mouseManager.IsRightClickDown = false;
                mouseManager.IsRightClickUp = true;
                var previousData = SystemAPI.GetComponent<MouseRightClickEvent>(mouseManagerEntity);
                SystemAPI.SetComponent(mouseManagerEntity, new MouseRightClickEvent
                {
                    LastPosition = previousData.Position,
                    Position = mousePosition,
                    LastPositionWorld = previousData.PositionWorld,
                    PositionWorld = mouseWorldPosition,
                    RightClickID = ++previousData.RightClickID
                });
            }
        }


        if (mouseManager.ActivateMiddleClickEventSystem)
        {
            if (Input.GetMouseButtonDown(2) && !mouseManager.IsMiddleClickDown)
            {
                mouseManager.IsMiddleClickDown = true;
            }
            else if (Input.GetMouseButtonUp(2) && mouseManager.IsMiddleClickDown)
            {
                mouseManager.IsMiddleClickDown = false;
                mouseManager.IsMiddleClickUp = true;
                var previousData = SystemAPI.GetComponent<MouseMiddleClickEvent>(mouseManagerEntity);
                SystemAPI.SetComponent(mouseManagerEntity, new MouseMiddleClickEvent
                {
                    LastPosition = previousData.Position,
                    Position = mousePosition,
                    LastPositionWorld = previousData.PositionWorld,
                    PositionWorld = mouseWorldPosition,
                    MiddleClickID = ++previousData.MiddleClickID
                });
            }
        }

        if (mouseManager.ActivateMouseMovementEventSystem)
        {
            var previousData = SystemAPI.GetComponent<MouseMovementEvent>(mouseManagerEntity);
            SystemAPI.SetComponent(mouseManagerEntity, new MouseMovementEvent
            {
                LastPosition = previousData.Position,
                Position = mousePosition,
                LastPositionWorld = previousData.PositionWorld,
                PositionWorld = mouseWorldPosition,
                MovementID = ++previousData.MovementID
            });
        }

        SystemAPI.SetComponent(mouseManagerEntity, mouseManager);
    }
}


public struct MouseManager : IComponentData
{
    public bool ActivateRightClickEventSystem;
    public bool ActivateLeftClickEventSystem;
    public bool ActivateMiddleClickEventSystem;
    public bool ActivateMouseMovementEventSystem;
    public float3 InitialClickPosition;
    public float3 FinalClickPosition;
    public float3 InitialClickPositionWorld;
    public float3 FinalClickPositionWorld;
    public bool IsLeftClickDown;
    public bool IsLeftClickUp;
    public bool IsRightClickDown;
    public bool IsRightClickUp;
    public bool IsMiddleClickDown;
    public bool IsMiddleClickUp;
}


public struct MouseRightClickEvent : IComponentData
{
    public float3 LastPosition;
    public float3 Position;
    public float3 LastPositionWorld;
    public float3 PositionWorld;
    public int RightClickID;
}

public struct MouseLeftClickEvent : IComponentData
{
    public float3 LastPosition;
    public float3 Position;
    public float3 LastPositionWorld;
    public float3 PositionWorld;
    public int LeftClickID;
}

public struct MouseMiddleClickEvent : IComponentData
{
    public float3 LastPosition;
    public float3 Position;
    public float3 LastPositionWorld;
    public float3 PositionWorld;
    public int MiddleClickID;
}

public struct MouseMovementEvent : IComponentData
{
    public float3 LastPosition;
    public float3 Position;
    public float3 LastPositionWorld;
    public float3 PositionWorld;
    public int MovementID;
}