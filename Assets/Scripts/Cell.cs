using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Cell : MonoBehaviour
{
    [SerializeField]
    internal CellType cellType;
    [SerializeField]
    internal List<Cell> neighbors;
    [SerializeField]
    internal float g_cost = Mathf.Infinity;
    [SerializeField]
    internal float f_cost = Mathf.Infinity;
    [SerializeField]
    internal Cell cameFrom;
    public Material baseMat;
    public Material pathMat;
    internal Renderer _renderer;

    internal float travelCost;
    public float TravelCost { get { return travelCost; } }
    public float G_Cost { get { return g_cost; } set { g_cost = value; } }
    public float F_Cost { get { return f_cost; } set { f_cost = value; } }
    public Cell CameFrom { get { return cameFrom; } set { cameFrom = value; } }
    public CellType CellType { get { return cellType; } }


    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    public void Initialize(CellType type)
    {
        cellType = type;
        travelCost = ComputeTravelCost();
        neighbors = new List<Cell>();
    }

    internal float ComputeTravelCost()
    {
        switch (cellType)
        {
            case CellType.Basic:
                return 1.0f;
            case CellType.Special: return 2.0f;
        }
        return 0.0f;
    }

    public List<Cell> GetNeighbors()
    {
        return neighbors;
    }

    public void AddNeighbor(GridCell cell)
    {
        neighbors.Add(cell);
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

    public Vector3 GetWorldPosition()
    {
        return transform.parent.position + transform.localPosition;
    }

    public abstract float Heuristic(Cell target);
}
