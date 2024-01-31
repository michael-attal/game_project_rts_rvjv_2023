using UnityEngine;

public class SpawnManagerWithoutECS : MonoBehaviour
{
    public Species PlayerOneSpecies;
    public Species PlayerTwoSpecies;
    public int NumberOfBaseSpawnerForPlayerOne;
    public int NumberOfBaseSpawnerForPlayerTwo;
    public Vector3 StartPositionBaseSpawnerPlayerOne;

    public Vector3 StartPositionBaseSpawnerPlayerTwo;
    // public bool SpawnUnitWhenPressEnter;

    public int BasicSlimeUnitPerBaseSpawner;
    public int BasicMecaUnitPerBaseSpawner;

    public GameObject SlimeBaseSpawnerBuildingPrefab;
    public GameObject MecaBaseSpawnerBuildingPrefab;
    public GameObject BasicSlimeUnitPrefab;
    public GameObject BasicMecaUnitPrefab;

    private void Start()
    {
        for (var i = 0; i < NumberOfBaseSpawnerForPlayerOne + NumberOfBaseSpawnerForPlayerTwo; i++)
        {
            var currentSpecies = i < NumberOfBaseSpawnerForPlayerOne ? PlayerOneSpecies : PlayerTwoSpecies;

            var positionBaseSpawner = currentSpecies == Species.Slime
                ? new Vector3(StartPositionBaseSpawnerPlayerOne.x + i % NumberOfBaseSpawnerForPlayerTwo,
                    StartPositionBaseSpawnerPlayerOne.y,
                    StartPositionBaseSpawnerPlayerOne.z + i % NumberOfBaseSpawnerForPlayerTwo)
                : new Vector3(StartPositionBaseSpawnerPlayerTwo.x + i % NumberOfBaseSpawnerForPlayerOne,
                    StartPositionBaseSpawnerPlayerTwo.y,
                    StartPositionBaseSpawnerPlayerTwo.z + i % NumberOfBaseSpawnerForPlayerOne);

            var BaseSpawner = Instantiate(
                currentSpecies == Species.Slime ? SlimeBaseSpawnerBuildingPrefab : MecaBaseSpawnerBuildingPrefab,
                positionBaseSpawner,
                Quaternion.identity);

            var buildingManager = BaseSpawner.AddComponent<BaseBuildingManagerWithoutECS>();
            buildingManager.NumberOfUnitToSpawn = currentSpecies == Species.Slime
                ? BasicSlimeUnitPerBaseSpawner
                : BasicMecaUnitPerBaseSpawner;
            buildingManager.BasicUnitPrefab =
                currentSpecies == Species.Slime ? BasicSlimeUnitPrefab : BasicMecaUnitPrefab;
        }
    }

    private void Update()
    {
    }
}