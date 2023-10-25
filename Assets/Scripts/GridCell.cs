using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CellType
{
    Basic, Special,
}
public class GridCell : MonoBehaviour
{
    [SerializeField]
    private CellType cellType;
    [SerializeField]
    private Vector2Int gridPosition;
    private Grid grid;
    [SerializeField]
    private List<GridCell> neighbors;
    [SerializeField]
    private float g_cost = Mathf.Infinity;
    [SerializeField]
    private float f_cost = Mathf.Infinity;
    [SerializeField]
    private GridCell cameFrom;
    public Material baseMat;
    public Material pathMat;
    [SerializeField]
    private Renderer _renderer;

    private float travelCost;
    public CellType CellType { get { return cellType; } }
    public Vector2Int GridPosition { get { return gridPosition; } }
    public float TravelCost { get { return travelCost; } }
    public float G_Cost { get { return g_cost; } set { g_cost = value; } }
    public float F_Cost { get { return f_cost; } set { f_cost = value; } }
    public GridCell CameFrom {  get { return cameFrom; } set {  cameFrom = value; } }

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();      
    }
    public void Initialize(Vector2Int position, CellType type, Grid grid)
    {
        gridPosition = position;
        cellType = type;
        this.grid = grid;
        travelCost = ComputeTravelCost();
        neighbors = new List<GridCell>();
    }
    private float ComputeTravelCost()
    {
        switch (cellType)
        {
            case CellType.Basic:
                return 1.0f;
            case CellType.Special: return 2.0f;
        }
        return 0.0f;
    }

    public void AddNeighbor(GridCell cell)
    {
        neighbors.Add(cell);
    }

    public Vector3 GetWorldPosition()
    {
        return transform.parent.position + transform.localPosition;
    }

    //returns h* for a given grid cell
    public float H_Star(GridCell target)
    {
        switch (grid.gridType)
        {
            case GridType.Connected4:
                return grid.ManhatanDistance(gridPosition, target.gridPosition);
            case GridType.Connected8:
                return grid.EuclidianDistance(gridPosition, target.gridPosition);
        }
        return 0;
    }

    public List<GridCell> GetNeighbors()
    {
        return neighbors;
    }

    public void AddToPath()
    {
        _renderer.material = pathMat;
    }

    public void ResetCell()
    {
        g_cost = Mathf.Infinity;
        f_cost = Mathf.Infinity;
        cameFrom = null;
        _renderer.material = baseMat;
    }
}
