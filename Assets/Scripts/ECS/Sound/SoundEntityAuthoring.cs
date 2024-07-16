using Unity.Entities;
using UnityEngine;

public class SoundEntityAuthoring : MonoBehaviour
{
    [SerializeField] private EntityType entityType;

    private class Baker : Baker<SoundEntityAuthoring>
    {
        public override void Bake(SoundEntityAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new Sound
            {
                EntityType = authoring.entityType,
                SoundToPlay = SoundType.None
            });
            SetComponentEnabled<Sound>(entity, false);
        }
    }
}

public enum EntityType
{
    None,
    SlimeBasicWaterUnit,
    SlimeStrongerWaterUnit,
    MecaBasicUnit,
    SlimeBaseSpawner,
    MecaBaseSpawner,
    UI
}

public enum SoundType
{
    None,
    Idle,
    Attack,
    Move,
    Damage,
    Death,
    Miscellaneous,
    Menu,
    Battlefield,
    Victory,
    Lost
}

public struct Sound : IComponentData, IEnableableComponent
{
    public EntityType EntityType;
    public SoundType SoundToPlay;
}