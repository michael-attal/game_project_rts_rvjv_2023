using UnityEngine;

// REMARK: If there is a significant difference in the base building manager between Slime and Meca, we need to create two separate scripts, such as BasicSlimeBaseBuildingManagerWithoutECS and BasicMecaBaseBuildingManagerWithoutECS
public class BaseBuildingManagerWithoutECS : MonoBehaviour
{
    public int NumberOfUnitToSpawn;
    public GameObject BasicUnitPrefab;

    private void Start()
    {
        var currentPosition = transform.position;

        var rows = 50;
        var cols = NumberOfUnitToSpawn / rows;
        var spacer = 2; // Space between 10x10 units group
        var unitSpace = 1.2f; // space between units

        for (var x = 0; x < rows; x++)
        for (var z = 0; z < cols; z++)
        {
            var extraXSpace = x % 10 == 0 ? spacer : 0;
            var extraZSpace = z % 10 == 0 ? spacer : 0;


            var basicUnit = Instantiate(BasicUnitPrefab,
                new Vector3(x * unitSpace + extraXSpace - rows / 2 - spacer * 2, currentPosition.y,
                    z * unitSpace + extraZSpace + currentPosition.z),
                Quaternion.identity);
            var basicUnitManager = basicUnit.AddComponent<BasicUnitManagerWithoutECS>();
        }
    }
}