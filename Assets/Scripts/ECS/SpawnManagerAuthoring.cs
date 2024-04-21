using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SpawnManagerAuthoring : MonoBehaviour
{
    // The SpawnManager component will be used as a singleton.
    // It stores a grab bag of game parameters plus the entity prefabs that we'll instantiate at runtime.
    public SpeciesType PlayerOneSpecies;
    public SpeciesType PlayerTwoSpecies;
    public float3 StartPositionBaseSpawnerPlayerOne;
    public float3 StartPositionBaseSpawnerPlayerTwo;

    public bool SpawnUnitWhenPressEnter;
    public GroupUnitShape GroupUnitsBy;

    public uint NumberOfBaseSpawnerForPlayerOne;
    public uint NumberOfBaseSpawnerForPlayerTwo;
    public uint NumberOfSlimeUnitPerSlimeBaseSpawner;
    public uint NumberOfMecaUnitPerMecaBaseSpawner;

    public GameObject SlimePlayerHandPrefab;
    public GameObject SlimeBaseSpawnerBuildingPrefab;
    public GameObject SlimeBasicUnitPrefab;
    public GameObject SlimeStrongerUnitPrefab;

    public GameObject MecaPlayerHandPrefab;
    public GameObject MecaBaseSpawnerBuildingPrefab;
    public GameObject MecaBasicUnitPrefab;

    public Material UnitSelectedCircleMaterial;

    public static Mesh CreateMesh(float meshWidth, float meshHeight)
    {
        var vertices = new Vector3[4];
        var uv = new Vector2[4];
        var triangles = new int[6];
        var meshWidthHalf = meshWidth / 2f;
        var meshHeightHalf = meshHeight / 2f;
        vertices[0] = new Vector3(-meshWidthHalf, meshHeightHalf);
        vertices[1] = new Vector3(meshWidthHalf, meshHeightHalf);
        vertices[2] = new Vector3(-meshWidthHalf, -meshHeightHalf);
        vertices[3] = new Vector3(meshWidthHalf, -meshHeightHalf);
        uv[0] = new Vector2(0, 1);
        uv[1] = new Vector2(1, 1);
        uv[2] = new Vector2(0, 0);
        uv[3] = new Vector2(1, 0);
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        triangles[3] = 2;
        triangles[4] = 1;
        triangles[5] = 3;
        var mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        return mesh;
    }


    private class Baker : Baker<SpawnManagerAuthoring>
    {
        public override void Bake(SpawnManagerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            // Each authoring field corresponds to a component field of the same name.
            AddComponentObject(entity, new SpawnManager
            {
                PlayerOneSpecies = authoring.PlayerOneSpecies,
                PlayerTwoSpecies = authoring.PlayerTwoSpecies,
                StartPositionBaseSpawnerPlayerOne = authoring.StartPositionBaseSpawnerPlayerOne,
                StartPositionBaseSpawnerPlayerTwo = authoring.StartPositionBaseSpawnerPlayerTwo,

                SpawnUnitWhenPressEnter = authoring.SpawnUnitWhenPressEnter,
                GroupUnitsBy = authoring.GroupUnitsBy,

                NumberOfBaseSpawnerForPlayerOne = authoring.NumberOfBaseSpawnerForPlayerOne,
                NumberOfBaseSpawnerForPlayerTwo = authoring.NumberOfBaseSpawnerForPlayerTwo,
                NumberOfSlimeUnitPerSlimeBaseSpawner = authoring.NumberOfSlimeUnitPerSlimeBaseSpawner,
                NumberOfMecaUnitPerMecaBaseSpawner = authoring.NumberOfMecaUnitPerMecaBaseSpawner,

                // SlimeBasicUnitSpeed = authoring.SlimeBasicUnitSpeed,
                // SlimeBasicUnitAttack = authoring.SlimeBasicUnitAttack,
                // MecaBasicUnitSpeed = authoring.MecaBasicUnitSpeed,
                // MecaBasicUnitAttack = authoring.MecaBasicUnitAttack,

                // GetEntity() bakes a GameObject prefab into its entity equivalent.
                SlimePlayerHandPrefab = GetEntity(authoring.SlimePlayerHandPrefab, TransformUsageFlags.Dynamic),
                SlimeBaseSpawnerBuildingPrefab = GetEntity(authoring.SlimeBaseSpawnerBuildingPrefab, TransformUsageFlags.Dynamic),
                SlimeBasicUnitPrefab = GetEntity(authoring.SlimeBasicUnitPrefab, TransformUsageFlags.Dynamic),
                SlimeStrongerUnitPrefab = GetEntity(authoring.SlimeStrongerUnitPrefab, TransformUsageFlags.Dynamic),

                MecaPlayerHandPrefab = GetEntity(authoring.MecaPlayerHandPrefab, TransformUsageFlags.Dynamic),
                MecaBaseSpawnerBuildingPrefab = GetEntity(authoring.MecaBaseSpawnerBuildingPrefab, TransformUsageFlags.Dynamic),
                MecaBasicUnitPrefab = GetEntity(authoring.MecaBasicUnitPrefab, TransformUsageFlags.Dynamic),

                UnitSelectedCircleMaterial = authoring.UnitSelectedCircleMaterial,
                UnitSelectedCircleMesh = CreateMesh(2f, 2f)
                //UnitSelectedCircleShadowMesh = CreateMesh(0.5f, 0.5f)
            });
        }
    }
}

public class SpawnManager : IComponentData
{
    public GroupUnitShape GroupUnitsBy;
    public Entity MecaBaseSpawnerBuildingPrefab;
    public float MecaBasicUnitAttack;
    public Entity MecaBasicUnitPrefab;
    public float MecaBasicUnitSpeed;

    public Entity MecaPlayerHandPrefab;

    public uint NumberOfBaseSpawnerForPlayerOne;
    public uint NumberOfBaseSpawnerForPlayerTwo;
    public uint NumberOfMecaUnitPerMecaBaseSpawner;
    public uint NumberOfSlimeUnitPerSlimeBaseSpawner;
    public SpeciesType PlayerOneSpecies;
    public SpeciesType PlayerTwoSpecies;
    public Entity SlimeBaseSpawnerBuildingPrefab;
    public float SlimeBasicUnitAttack; // Amount of damage dealt by the slime
    public Entity SlimeBasicUnitPrefab;

    public float SlimeBasicUnitSpeed; // meters per second
    // SlimeBasicUnitLife, MecaBasicUnitLife ...

    public Entity SlimePlayerHandPrefab;
    public Entity SlimeStrongerUnitPrefab;

    public bool SpawnUnitWhenPressEnter;
    public float3 StartPositionBaseSpawnerPlayerOne;
    public float3 StartPositionBaseSpawnerPlayerTwo;

    public Material UnitSelectedCircleMaterial;
    public Mesh UnitSelectedCircleMesh;

    //public Mesh UnitSelectedCircleShadowMesh;
}