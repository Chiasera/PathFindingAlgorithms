using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AgentAI : MonoBehaviour
{
    protected Cell currentCell;
    protected Cell nextCell;
    [SerializeField]
    [Range(0.1f, 5)]
    protected float movementSpeed;
    protected Rigidbody rb;
    protected bool isAsleep = true;
    [SerializeField]
    [Range(0.1f, 50)]
    protected float rotationSpeed;

    public float MovementSpeed { get { return movementSpeed; } }
    public Cell CurrentCell { get { return currentCell; } set { currentCell = value; } }

    protected virtual void SwitchCells(Cell newCell)
    {
        if(newCell.CellType != CellType.Goal)
        {
            currentCell.CellType = CellType.Basic;
            currentCell = newCell;        
            currentCell.CellType = CellType.Obstacle;
        }     
    }

    public void SetCurrentCell(Cell cell)
    {
        currentCell = cell;
        currentCell.IsBooked = true;
    }


    protected void OnCellChange()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            try
            {
                Cell collidedCell = hit.collider.GetComponent<Cell>();
                if (collidedCell != null)
                {
                    SwitchCells(collidedCell);
                }
            }
            catch
            {
                //ignore
            }
        }
    }
}
