using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CellType
{
    Basic, Special, Obstacle, Goal
}
public abstract class Cell : MonoBehaviour
{
    [SerializeField]
    protected CellType cellType;
    [SerializeField]
    protected List<Cell> neighbors;
    public Material baseMat;
    public Material pathMat;
    public Material goalMat;
    public Material obstacleMat;
    protected Renderer _renderer;
    protected bool isBooked;

    protected float travelCost;
    public bool IsBooked { get { return isBooked; } set { isBooked = value; } }
    public float TravelCost { get { return travelCost; } }
    public CellType CellType
    {
        get { return cellType; }
        set
        {
            cellType = value;
            UpdateMaterial();
            UpdateTravelCost();
        }
    }

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    public void Initialize(CellType type)
    {
        cellType = type;
        neighbors = new List<Cell>();
    }

    //Update the material -- for debug purposes
    private void UpdateMaterial()
    {
        switch (cellType)
        {
            case CellType.Obstacle:
                _renderer.material = obstacleMat;
                break;
            case CellType.Goal:
                _renderer.material = goalMat;
                break;
        }
    }

    protected void UpdateTravelCost()
    {
        switch (cellType)
        {
            case CellType.Basic: travelCost = 1.0f;
                break;
            case CellType.Special: travelCost =  2.0f;
                break;
            case CellType.Obstacle: travelCost = Mathf.Infinity;
                break;
            case CellType.Goal: travelCost = 1.0f;
                break;
        }
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
        if (this.cellType != CellType.Goal)
        {
            _renderer.material = pathMat;
        }
    }

    public void ResetCell()
    {
        _renderer.material = baseMat;
    }

    public Vector3 GetWorldPosition()
    {
        return transform.parent.position + transform.localPosition;
    }

    public abstract float Heuristic(Cell target);
}
