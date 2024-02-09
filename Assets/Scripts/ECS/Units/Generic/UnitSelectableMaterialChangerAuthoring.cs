using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class UnitSelectableMaterialChanger : IComponentData
{
    public uint active;
    public Material UnitDefaultMaterial;
    public Material UnitSelectedMaterial;
}

[DisallowMultipleComponent]
public class UnitSelectableMaterialChangerAuthoring : MonoBehaviour
{
    public Material UnitDefaultMaterial;
    public Material UnitSelectedMaterial;

    [RegisterBinding(typeof(UnitSelectableMaterialChanger), "active")]
    public uint active;

    private class MaterialChangerBaker : Baker<UnitSelectableMaterialChangerAuthoring>
    {
        public override void Bake(UnitSelectableMaterialChangerAuthoring authoring)
        {
            var component = new UnitSelectableMaterialChanger();
            component.UnitDefaultMaterial = authoring.UnitDefaultMaterial;
            component.UnitSelectedMaterial = authoring.UnitSelectedMaterial;
            component.active = authoring.active;
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, component);
        }
    }
}

[RequireMatchingQueriesForUpdate]
public partial class UnitSelectableMaterialChangerSystem : SystemBase
{
    private Dictionary<Material, BatchMaterialID> m_MaterialMapping;

    private void RegisterMaterial(EntitiesGraphicsSystem hybridRendererSystem, Material material)
    {
        // Only register each mesh once, so we can also unregister each mesh just once
        if (!m_MaterialMapping.ContainsKey(material))
            m_MaterialMapping[material] = hybridRendererSystem.RegisterMaterial(material);
    }

    protected override void OnStartRunning()
    {
        var hybridRenderer = World.GetOrCreateSystemManaged<EntitiesGraphicsSystem>();
        m_MaterialMapping = new Dictionary<Material, BatchMaterialID>();

        Entities
            .WithoutBurst()
            .ForEach((in UnitSelectableMaterialChanger changer) =>
            {
                RegisterMaterial(hybridRenderer, changer.UnitDefaultMaterial);
                RegisterMaterial(hybridRenderer, changer.UnitSelectedMaterial);
            }).Run();
    }

    private void UnregisterMaterials()
    {
        // Can't call this from OnDestroy(), so we can't do this on teardown
        var hybridRenderer = World.GetExistingSystemManaged<EntitiesGraphicsSystem>();
        if (hybridRenderer == null)
            return;

        foreach (var kv in m_MaterialMapping)
            hybridRenderer.UnregisterMaterial(kv.Value);
    }

    protected override void OnUpdate()
    {
        var configManager = SystemAPI.GetSingleton<Config>();

        if (!configManager.ActivateSelectableMaterialChangerSystem)
            return;

        var entityManager = EntityManager;

        if (!Input.GetKeyDown(KeyCode.M)) // Change material by pressing M for the moment.
            return;

        Entities
            .WithoutBurst()
            .ForEach((UnitSelectableMaterialChanger changer, ref MaterialMeshInfo mmi) =>
            {
                var material = changer.active == 0 ? changer.UnitDefaultMaterial : changer.UnitSelectedMaterial;
                mmi.MaterialID = m_MaterialMapping[material];
            }).Run();
    }
}