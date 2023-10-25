using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GridType
{
    Connected4, Connected8
}

public static class Direction2D
{
    public static IEnumerable<Vector2Int> AllDirections => new List<Vector2Int>
    {
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(0, 1)
    };
}

public class Grid : MonoBehaviour
{
    //assuming no duplicate cells. No collisions within our hash structure. 
    //If we wanted to handle collisions similarly to how java's hashmap does: Dictionary<Vector2Int, List<int>>
    //But in our case, each grid cell is unique, so it ensures constant access time O(1) for a grid cell
    [SerializeField]
    private Dictionary<Vector2Int, GridCell> cells;
    [SerializeField]
    private GameObject cellPrefab;
    [Range(0, 10)]
    public float cellSpacing;
    public GridType gridType = GridType.Connected4;
    public List<GridCell> path;
    private void Awake()
    {
        cells = new Dictionary<Vector2Int, GridCell>();
        GenerateGrid(4, GridType.Connected4);
        A_StarSearch alg = new A_StarSearch(this, cells[new Vector2Int(0,0)], cells[new Vector2Int(2, 3)]);
        path = alg.StartSearch();
        foreach(var cell in path)
        {
            cell.AddToPath();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    //generate a grid of size n
    public void GenerateGrid(int gridSize, GridType type)
    {
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                //h* is not necessarily unique, but the gridCell position is
                //key cannot be changed, value can, the heuristic will be adjusted later on
                Vector2Int gridPosition = new Vector2Int(i, j);
                GridCell newCell = Instantiate(cellPrefab,
                    transform.parent.position + new Vector3(i + i * cellSpacing, 0, j + j * cellSpacing),
                    Quaternion.identity).GetComponent<GridCell>();
                newCell.transform.parent = transform.parent;
                //first pass: every cell initialized as a basic cell
                newCell.Initialize(gridPosition, CellType.Basic, this);
                cells.Add(gridPosition, newCell);
            }
        }
        AssignCellNeighbors();
    }

    private void AssignCellNeighbors()
    {
        foreach (GridCell cell in cells.Values)
        {
            foreach (var direction in Direction2D.AllDirections)
            {
                GridCell neighbor;
                cells.TryGetValue(cell.GridPosition + direction, out neighbor);
                if (neighbor != null)
                {
                    cell.AddNeighbor(neighbor);
                }
            }
        }
    }

    public int ManhatanDistance(Vector2Int position1, Vector2Int position2)
    {
        return Mathf.Abs(position1.x - position2.x) + Mathf.Abs(position1.y - position2.y);
    }

    public float EuclidianDistance(Vector2Int position1, Vector2Int position2)
    {
        int a = position1.x - position2.x;
        int b = position1.y - position2.y;
        return Mathf.Sqrt(a * a + b * b);
    }
}
