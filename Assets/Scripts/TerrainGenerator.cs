using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public GameObject treePrefab; // Assign your tree prefab here
    public int seed = 12345; // Seed for random state initialization
    public int treeCount = 100; // Number of trees to spawn
    public float spawnRadius = 50f; // Maximum distance from the center of the terrain to spawn trees

    void Start()
    {
        SpawnTrees();
    }

    void SpawnTrees()
    {
        Random.InitState(seed); // Initialize the random state with the seed

        for (int i = 0; i < treeCount; i++)
        {
            // Generate a random position within the spawn radius
            Vector3 position = new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                0,
                Random.Range(-spawnRadius, spawnRadius)
            );

            // Instantiate the tree at the generated position
            Instantiate(treePrefab, position, Quaternion.identity);
        }
    }
}

