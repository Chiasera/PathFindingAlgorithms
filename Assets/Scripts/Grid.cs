using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GridType
{
    Connected4, Connected8
}

public static class Direction2D
{
    public static IEnumerable<Vector2Int> Direction8By8 => new List<Vector2Int>
    {
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(0, 1),
        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1),
    };
    public static IEnumerable<Vector2Int> Direction4By4 => new List<Vector2Int>
    {
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(0, 1),
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
    public GridType gridType;
    public Stack<Cell> path;
    [Range(1, 100)]
    public int gridSize_N;
    [Range(1, 100)]
    public int gridSize_M;
    [Range(1, 500)]
    public int removeCells;

    public Dictionary<Vector2Int, GridCell> Cells {  get { return cells; } }
    private void Awake()
    {
        cells = new Dictionary<Vector2Int, GridCell>();
        GenerateGrid(gridSize_N, gridSize_M, GridType.Connected8);      
        //RandomA_StarSearch();
    }

    private void RandomA_StarSearch()
    {
        Vector2Int startPosition = new Vector2Int(Random.Range(0, gridSize_N - 1), Random.Range(0, gridSize_N - 1));
        Vector2Int targetPosition = new Vector2Int(Random.Range(0, gridSize_N - 1), Random.Range(0, gridSize_N - 1));
        //------------------ Create some holes within the grid, mostly for debug purposes ----------------------------
        /*for (int i = 0; i < removeCells; i++)
        {
            Vector2Int removePosition = new Vector2Int(Random.Range(0, gridSize_N - 1), Random.Range(0, gridSize_N - 1));
            if (removePosition != startPosition && removePosition != targetPosition)
            {
                try
                {
                    GridCell cell = cells[removePosition];
                    foreach (GridCell neighbor in cell.GetNeighbors())
                    {
                        neighbor.GetNeighbors().Remove(cell);
                    }
                    Destroy(cells[removePosition].gameObject);
                }
                catch
                {
                    //ignore
                }
            }
        }*/
        //----------------------------------------------------------------------------------------------------------
        A_StarSearch alg = new A_StarSearch(cells[startPosition], cells[targetPosition]);
        path = alg.StartSearch();
        foreach (var cell in path)
        {
            cell.AddToPath();
        }
    }


    //generate a grid of size n
    public void GenerateGrid(int gridSize_N, int gridSize_M, GridType type)
    {
        gridType = type;
        for (int i = 0; i < gridSize_N; i++)
        {
            for (int j = 0; j < gridSize_M; j++)
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
            foreach (var direction in Direction2D.Direction8By8)
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
        Debug.Log(Mathf.Sqrt(a * a + b * b));
        return Mathf.Sqrt(a * a + b * b);
    }
}
