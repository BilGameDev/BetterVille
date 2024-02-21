using UnityEngine;
using System.Collections.Generic;
using Pathfinding;
using Unity.VisualScripting;

public class GridManager : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject[] treePrefabs;
    public float waterLevel = .4f;
    public float scale = .1f;
    public float treeNoiseScale = .05f;
    public float treeDensity = .5f;
    public float riverNoiseScale = .06f;
    public int rivers = 5;
    public int size = 100;
    public int seed = 12345;
    public Vector2 moveSpeed;

    Cell[,] grid;

    public bool UpdateIsland;
    float[,] noiseMap;
    float[,] falloffMap;
    float xOffset = 0;
    float yOffset = 0;
    private float updateInterval = 0.5f; // Time in seconds between updates
    private float nextUpdateTime = 0f; // When the next update should occur

    void Update()
    {
        // Check if the current time has reached the next scheduled update time
        if (UpdateIsland && Time.time >= nextUpdateTime)
        {
            ResetTiles();
            ConfigureIsland();
            SpawnIsland();
            // Schedule the next update
            nextUpdateTime = Time.time + updateInterval;
        }
    }

    void SpawnIsland()
    {
        //GenerateRivers(grid);
        DrawTerrainMesh3D(grid);
        DrawEdgeMesh3D(grid);
        //GenerateTrees(grid);
    }

    void Start()
    {
        // Initialize the random state with the seed
        Random.InitState(seed);
    }

    void GenerateRivers(Cell[,] grid)
    {
        float[,] noiseMap = new float[size, size];
        (float xOffset, float yOffset) = (Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float noiseValue = Mathf.PerlinNoise(x * riverNoiseScale + xOffset, y * riverNoiseScale + yOffset);
                noiseMap[x, y] = noiseValue;
            }
        }

        GridGraph gg = AstarData.active.graphs[0] as GridGraph;
        gg.center = new Vector3(size / 2f - .5f, 0, size / 2f - .5f);
        gg.SetDimensions(size, size, 1);
        AstarData.active.Scan(gg);
        AstarData.active.AddWorkItem(new AstarWorkItem(ctx =>
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    GraphNode node = gg.GetNode(x, y);
                    node.Walkable = noiseMap[x, y] > .4f;
                }
            }
        }));
        AstarData.active.FlushGraphUpdates();

        int k = 0;
        for (int i = 0; i < rivers; i++)
        {
            GraphNode start = gg.nodes[Random.Range(16, size - 16)];
            GraphNode end = gg.nodes[Random.Range(size * (size - 1) + 16, size * size - 16)];
            ABPath path = ABPath.Construct((Vector3)start.position, (Vector3)end.position, (Path result) =>
            {
                for (int j = 0; j < result.path.Count; j++)
                {
                    GraphNode node = result.path[j];
                    int x = Mathf.RoundToInt(((Vector3)node.position).x);
                    int y = Mathf.RoundToInt(((Vector3)node.position).z);
                    grid[x, y].isWater = true;
                }
                k++;
            });
            AstarPath.StartPath(path);
            AstarPath.BlockUntilCalculated(path);
        }
    }

    void DrawTerrainMesh3D(Cell[,] grid)
    {
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Cell cell = grid[x, y];
                // Skip corner pieces
                if (IsCorner(x, y, grid))
                {
                    continue;
                }

                bool Spawn = cell.isWater ? false : true;
                Vector3 position = new Vector3(x, 0, y);
                if (Spawn)
                {
                    GameObject centerTile = gameManager.centerTile.GetPooledObject();
                    centerTile.transform.SetPositionAndRotation(position, Quaternion.identity);
                }
            }
        }
    }

    void ResetTiles()
    {
        foreach (Transform child in gameManager.centerTile.transform)
        {
            child.gameObject.SetActive(false);
        }

        foreach (Transform child in gameManager.edgeTile.transform)
        {
            child.gameObject.SetActive(false);
        }

        foreach (Transform child in gameManager.cornerTile.transform)
        {
            child.gameObject.SetActive(false);
        }

        foreach (Transform child in gameManager.soloTile.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    void ConfigureIsland()
    {
        noiseMap = new float[size, size];
        xOffset += Time.deltaTime * moveSpeed.x;
        yOffset += Time.deltaTime * moveSpeed.y;
        //(float xOffset, float yOffset) = (Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float noiseValue = Mathf.PerlinNoise((x + xOffset) * scale, (y + yOffset) * scale);
                noiseMap[x, y] = noiseValue;
            }
        }

        falloffMap = new float[size, size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float xv = x / (float)size * 2 - 1;
                float yv = y / (float)size * 2 - 1;
                float v = Mathf.Max(Mathf.Abs(xv), Mathf.Abs(yv));
                falloffMap[x, y] = Mathf.Pow(v, 3f) / (Mathf.Pow(v, 3f) + Mathf.Pow(2.2f - 2.2f * v, 3f));
            }
        }

        grid = new Cell[size, size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float noiseValue = noiseMap[x, y];
                noiseValue -= falloffMap[x, y];
                bool isWater = noiseValue < waterLevel;
                Cell cell = new Cell(isWater);
                grid[x, y] = cell;
            }
        }
    }


    void DrawEdgeMesh3D(Cell[,] grid)
    {
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Cell cell = grid[x, y];
                if (!cell.isWater)
                {
                    // Check adjacent cells to determine if this is an edge or corner
                    bool leftWater = x > 0 && grid[x - 1, y].isWater;
                    bool rightWater = x < size - 1 && grid[x + 1, y].isWater;
                    bool downWater = y > 0 && grid[x, y - 1].isWater;
                    bool upWater = y < size - 1 && grid[x, y + 1].isWater;

                    // Count how many sides are adjacent to water
                    int waterSides = 0;
                    waterSides += leftWater ? 1 : 0;
                    waterSides += rightWater ? 1 : 0;
                    waterSides += downWater ? 1 : 0;
                    waterSides += upWater ? 1 : 0;

                    Vector3 position = new Vector3(x, 0, y);
                    Quaternion rotation = Quaternion.identity;

                    // Determine the correct prefab and rotation based on adjacent water
                    GameObject prefabToUse = null;
                    int Spawn = 0;
                    if (waterSides == 1)
                    {
                        Spawn = 1;
                        // Determine rotation based on which side the water is on
                        if (leftWater) rotation = Quaternion.Euler(0, -90, 0);
                        else if (rightWater) rotation = Quaternion.Euler(0, 90, 0);
                        else if (downWater) rotation = Quaternion.Euler(0, 180, 0);
                        // No rotation needed if water is up
                    }
                    else if (waterSides == 2)
                    {
                        Spawn = 2;
                        // Use corner prefab if two adjacent sides are water
                        // Determine rotation based on which sides the water is on
                        if (leftWater && upWater) { rotation = Quaternion.Euler(0, -90, 0); }
                        else if (leftWater && downWater) { rotation = Quaternion.Euler(0, 180, 0); }
                        else if (rightWater && downWater) { rotation = Quaternion.Euler(0, 90, 0); }
                        // No rotation needed if water is right and up
                    }
                    else if (waterSides > 2)
                    {
                        Spawn = 3;

                    }

                    if (Spawn != 0)
                    {
                        switch (Spawn)
                        {
                            case 1:
                                prefabToUse = gameManager.edgeTile.GetPooledObject();
                                break;

                            case 2:
                                prefabToUse = gameManager.cornerTile.GetPooledObject();
                                break;

                            case 3:
                                prefabToUse = gameManager.soloTile.GetPooledObject();
                                break;
                        }

                        prefabToUse.transform.SetPositionAndRotation(position, rotation);
                    }
                }
            }
        }
    }

    bool IsCorner(int x, int y, Cell[,] grid)
    {
        // Check if the current cell is land and has two adjacent water cells forming a right angle
        bool currentIsLand = !grid[x, y].isWater;

        bool leftWater = x > 0 && grid[x - 1, y].isWater;
        bool rightWater = x < size - 1 && grid[x + 1, y].isWater;
        bool downWater = y > 0 && grid[x, y - 1].isWater;
        bool upWater = y < size - 1 && grid[x, y + 1].isWater;

        // Count the number of adjacent water tiles
        int waterSides = 0;
        waterSides += leftWater ? 1 : 0;
        waterSides += rightWater ? 1 : 0;
        waterSides += downWater ? 1 : 0;
        waterSides += upWater ? 1 : 0;

        // Determine if it's a corner based on the position of water cells
        // A corner is identified if there are exactly two adjacent water cells and those cells are perpendicular
        bool isCorner = currentIsLand && waterSides == 2 &&
                        ((leftWater && (upWater || downWater)) ||
                         (rightWater && (upWater || downWater)));

        return isCorner;
    }

    void GenerateTrees(Cell[,] grid)
    {
        float[,] noiseMap = new float[size, size];
        (float xOffset, float yOffset) = (Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float noiseValue = Mathf.PerlinNoise(x * treeNoiseScale + xOffset, y * treeNoiseScale + yOffset);
                noiseMap[x, y] = noiseValue;
            }
        }

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Cell cell = grid[x, y];
                if (!cell.isWater)
                {
                    float v = Random.Range(0f, treeDensity);
                    if (noiseMap[x, y] < v)
                    {
                        GameObject prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                        GameObject tree = Instantiate(prefab, transform);
                        tree.transform.position = new Vector3(x, 0, y);
                        tree.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                        tree.transform.localScale = Vector3.one * Random.Range(.8f, 1.2f);
                    }
                }
            }
        }
    }
}