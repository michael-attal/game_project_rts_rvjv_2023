using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Scripting;

public class MouseManagerAuthoring : MonoBehaviour
{
    [SerializeField] private bool activateRightClickEventSystem;

    private class Baker : Baker<MouseManagerAuthoring>
    {
        public override void Bake(MouseManagerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new MouseManager
            {
                ActivateRightClickEventSystem = authoring.activateRightClickEventSystem
            });

            AddComponent(entity, new MouseRightClickEvent
            {
                LastPosition = float3.zero,
                Position = float3.zero,
                RightClickID = -1
            });
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
public partial struct MouseRightClickSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MouseManager>();
        state.RequireForUpdate<MouseRightClickEvent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var mouseManager = SystemAPI.GetSingleton<MouseManager>();

        if (!mouseManager.ActivateRightClickEventSystem)
        {
            state.Enabled = false;
            return;
        }

        if (!Input.GetMouseButtonDown(1))
            return;

        var mouseManagerEntity = SystemAPI.GetSingletonEntity<MouseManager>();
        var clickPos = Input.mousePosition;
        var mainCamera = Camera.main;
        if (mainCamera == null)
            return;
        var clickWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(clickPos.x, clickPos.y, mainCamera.transform.position.y)); // TODO: Maybe do a calculation with the camera rotation instead of using y axis.

        var previousData = state.EntityManager.GetComponentData<MouseRightClickEvent>(mouseManagerEntity);

        state.EntityManager.SetComponentData(mouseManagerEntity, new MouseRightClickEvent
        {
            LastPosition = previousData.Position,
            Position = clickWorldPosition,
            RightClickID = ++previousData.RightClickID
        });
    }
}

public struct MouseManager : IComponentData
{
    public bool ActivateRightClickEventSystem;
}

public struct MouseRightClickEvent : IComponentData
{
    public float3 LastPosition;
    public float3 Position;
    public int RightClickID;
}

public struct MouseLeftClickEvent : IComponentData
{
    public float3 LastPosition;
    public float3 Position;
    public int LeftClickID;
}

public struct MouseMiddleClickEvent : IComponentData
{
    public float3 LastPosition;
    public float3 Position;
    public int MiddleClickID;
}

public struct MouseMovementEvent : IComponentData
{
    public float3 LastPosition;
    public float3 Position;
    public int MovementID;
}