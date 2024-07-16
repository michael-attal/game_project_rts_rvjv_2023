using Unity.Entities;
using UnityEngine;

public class SoundManagerAuthoring : MonoBehaviour
{
    [SerializeField] private float volume;

    private class Baker : Baker<SoundManagerAuthoring>
    {
        public override void Bake(SoundManagerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new SoundManager
            {
                Volume = authoring.volume
            });

            AddComponent(entity, new Sound
            {
                EntityType = EntityType.UI,
                SoundToPlay = SoundType.None
            });
            SetComponentEnabled<Sound>(entity, false);
        }
    }
}

public struct SoundManager : IComponentData
{
    public float Volume;
    public float BackgroundVolume;
}