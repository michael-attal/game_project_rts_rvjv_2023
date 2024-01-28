using Unity.Entities;
using UnityEngine;

public class ExecuteSpawnManagerAuthoring : MonoBehaviour
{
    [Header("Spawner Settings")]
    //
    public bool PlayerSpawner;

    public bool AutoSpawnIntervalUnit;


    [Header("BasicSlime Unit Settings")]
    //
    public bool BasicSlimeSpawner;

    public bool BasicSlimeMovement;
    public bool BasicSlimeAttack;
    public bool BasicSlimeDamage;
    public bool BasicSlimeHandled;

    [Header("BasicMeca Unit Settings")]
    //
    public bool BasicMecaSpawner;

    public bool BasicMecaMovement;
    public bool BasicMecaAttack;
    public bool BasicMecaDamage;
    public bool BasicMecaHandled;

    private class Baker : Baker<ExecuteSpawnManagerAuthoring>
    {
        public override void Bake(ExecuteSpawnManagerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            if (authoring.PlayerSpawner) AddComponent<PlayerSpawner>(entity);
            if (authoring.AutoSpawnIntervalUnit) AddComponent<AutoSpawnIntervalUnit>(entity);
            if (authoring.BasicSlimeSpawner) AddComponent<BasicSlimeSpawner>(entity);
            if (authoring.BasicSlimeMovement) AddComponent<BasicSlimeMovement>(entity);
            if (authoring.BasicSlimeAttack) AddComponent<BasicSlimeAttack>(entity);
            if (authoring.BasicSlimeDamage) AddComponent<BasicSlimeDamage>(entity);
            if (authoring.BasicSlimeHandled) AddComponent<BasicSlimeHandled>(entity);
            if (authoring.BasicMecaSpawner) AddComponent<BasicMecaSpawner>(entity);
            if (authoring.BasicMecaMovement) AddComponent<BasicMecaMovement>(entity);
            if (authoring.BasicMecaAttack) AddComponent<BasicMecaAttack>(entity);
            if (authoring.BasicMecaDamage) AddComponent<BasicMecaDamage>(entity);
            if (authoring.BasicMecaHandled) AddComponent<BasicMecaHandled>(entity);
        }
    }
}

public struct PlayerSpawner : IComponentData
{
}

public struct AutoSpawnIntervalUnit : IComponentData
{
}

public struct BasicSlimeSpawner : IComponentData
{
}

public struct BasicSlimeMovement : IComponentData
{
}

public struct BasicSlimeAttack : IComponentData
{
}

public struct BasicSlimeDamage : IComponentData
{
}

public struct BasicSlimeHandled : IComponentData
{
}

public struct BasicMecaSpawner : IComponentData
{
}

public struct BasicMecaMovement : IComponentData
{
}

public struct BasicMecaAttack : IComponentData
{
}

public struct BasicMecaDamage : IComponentData
{
}

public struct BasicMecaHandled : IComponentData
{
}