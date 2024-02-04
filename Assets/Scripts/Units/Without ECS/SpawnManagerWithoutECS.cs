using UnityEngine;

public class SpawnManagerWithoutECS : MonoBehaviour
{
    public SpeciesType PlayerOneSpecies;
    public SpeciesType PlayerTwoSpecies;
    public int NumberOfBaseSpawnerForPlayerOne;
    public int NumberOfBaseSpawnerForPlayerTwo;
    public Vector3 StartPositionBaseSpawnerPlayerOne;

    public Vector3 StartPositionBaseSpawnerPlayerTwo;
    // public bool SpawnUnitWhenPressEnter;

    public int BasicSlimeUnitPerBaseSpawner;
    public int BasicMecaUnitPerBaseSpawner;

    public GameObject SlimeBaseSpawnerBuildingPrefab;
    public GameObject MecaBaseSpawnerBuildingPrefab;
    public GameObject SlimeBasicUnitPrefab;
    public GameObject MecaBasicUnitPrefab;

    private void Start()
    {
        for (var i = 0; i < NumberOfBaseSpawnerForPlayerOne + NumberOfBaseSpawnerForPlayerTwo; i++)
        {
            var currentSpecies = i < NumberOfBaseSpawnerForPlayerOne ? PlayerOneSpecies : PlayerTwoSpecies;

            var positionBaseSpawner = currentSpecies == SpeciesType.Slime
                ? new Vector3(StartPositionBaseSpawnerPlayerOne.x + i % NumberOfBaseSpawnerForPlayerTwo,
                    StartPositionBaseSpawnerPlayerOne.y,
                    StartPositionBaseSpawnerPlayerOne.z + i % NumberOfBaseSpawnerForPlayerTwo)
                : new Vector3(StartPositionBaseSpawnerPlayerTwo.x + i % NumberOfBaseSpawnerForPlayerOne,
                    StartPositionBaseSpawnerPlayerTwo.y,
                    StartPositionBaseSpawnerPlayerTwo.z + i % NumberOfBaseSpawnerForPlayerOne);

            var BaseSpawner = Instantiate(
                currentSpecies == SpeciesType.Slime ? SlimeBaseSpawnerBuildingPrefab : MecaBaseSpawnerBuildingPrefab,
                positionBaseSpawner,
                Quaternion.identity);

            var buildingManager = BaseSpawner.AddComponent<BaseBuildingManagerWithoutECS>();
            buildingManager.NumberOfUnitToSpawn = currentSpecies == SpeciesType.Slime
                ? BasicSlimeUnitPerBaseSpawner
                : BasicMecaUnitPerBaseSpawner;
            buildingManager.BasicUnitPrefab =
                currentSpecies == SpeciesType.Slime ? SlimeBasicUnitPrefab : MecaBasicUnitPrefab;
        }
    }

    private void Update()
    {
    }
}