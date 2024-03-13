using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameManager gameManager;

    public int width = 5;
    public int height = 10;
    public float spacing = 0.7f;
    [SerializeField] GameObject[] candyPrefabs;
    private GameObject[,] grid;

    private GameObject matchSound;

    void Start()
    {
        matchSound = GameObject.Find("MatchSound");
        grid = new GameObject[width, height];
        FillGrid();
    }

    void FillGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                SpawnCandy(x, y);
            }
        }
    }

    void SpawnCandy(int x, int y)
    {
        int randomIndex = Random.Range(0, candyPrefabs.Length);
        GameObject selectedCandy = Instantiate(candyPrefabs[randomIndex], new Vector3(x * spacing, y * spacing, 0), Quaternion.identity);
        selectedCandy.transform.parent = this.transform;
        grid[x, y] = selectedCandy;
    }

    void CheckAndDestroyMatches()
    {
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

        if (hit.collider != null)
        {
            Vector2Int hitPosition = FindCandyPosition(hit.collider.gameObject);
            if (hitPosition.x != -1) // Ensure the candy is part of the grid
            {
                List<GameObject> matchedCandies = GetAdjacentMatches(hitPosition.x, hitPosition.y, hit.collider.gameObject.tag);
                if (matchedCandies.Count >= 2) // Ensure there's at least a match of 2
                {
                    // Add the initially tapped candy to the list for destruction if not already included
                    if (!matchedCandies.Contains(hit.collider.gameObject))
                    {
                        matchedCandies.Add(hit.collider.gameObject);
                    }

                    foreach (GameObject candy in matchedCandies)
                    {
                        Destroy(candy);
                        grid[FindCandyPosition(candy).x, FindCandyPosition(candy).y] = null;
                    }
                    gameManager.AddScore(10 * matchedCandies.Count); // Update score based on the number of destroyed candies
                    matchSound.GetComponent<AudioSource>().Play();
                    FillEmptySpaces();
                }
            }
        }
    }

    List<GameObject> GetAdjacentMatches(int x, int y, string tag)
    {
        List<GameObject> matchedCandies = new List<GameObject>();
        Queue<Vector2Int> checkQueue = new Queue<Vector2Int>();
        HashSet<Vector2Int> checkedPositions = new HashSet<Vector2Int>();

        checkQueue.Enqueue(new Vector2Int(x, y));

        while (checkQueue.Count > 0)
        {
            Vector2Int pos = checkQueue.Dequeue();
            if (checkedPositions.Contains(pos))
                continue;

            if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height && grid[pos.x, pos.y] != null && grid[pos.x, pos.y].tag == tag)
            {
                matchedCandies.Add(grid[pos.x, pos.y]);
                checkQueue.Enqueue(new Vector2Int(pos.x + 1, pos.y));
                checkQueue.Enqueue(new Vector2Int(pos.x - 1, pos.y));
                checkQueue.Enqueue(new Vector2Int(pos.x, pos.y + 1));
                checkQueue.Enqueue(new Vector2Int(pos.x, pos.y - 1));
            }

            checkedPositions.Add(pos);
        }

        return matchedCandies;
    }

    Vector2Int FindCandyPosition(GameObject candy)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == candy)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return new Vector2Int(-1, -1); // Candy not found in grid
    }





    void FillEmptySpaces()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null)
                {
                    SpawnCandy(x, y);
                }
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CheckAndDestroyMatches();
            gameManager.UseMove();
        }
    }

}
