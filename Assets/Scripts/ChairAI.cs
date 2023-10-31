using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChairAI: AgentAI 
{
    private AgentAI targetHuman;
    private Vector3 targetPosition;
    private float distanceForecast = 5;
    // Start is called before the first frame update

    public AgentAI GetClosestHuman(List<AgentAI> humans)
    {
        float closestDistance = Mathf.Infinity;
        if (GameManager.humans.Count > 0)
        {
            AgentAI closestHuman = GameManager.humans[0];
            foreach (HumanAI human in humans)
            {
                if (Vector3.Distance(transform.position, human.transform.position) < closestDistance)
                {
                    closestHuman = human;
                }
            }
            return closestHuman;
        }
        return null;
    }

    public void Activate(List<AgentAI> humans)
    {
        targetHuman = GetClosestHuman(humans);
    }

    private void Update()
    {
        OnCellChange();
        if (targetHuman != null)
        {
            targetPosition = targetHuman.transform.position + targetHuman.transform.forward * distanceForecast;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * movementSpeed);
            // Calculate the target rotation based on the velocity direction
            Quaternion targetRotation = Quaternion.LookRotation(targetHuman.transform.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        } else
        {
            targetHuman = GetClosestHuman(GameManager.humans);
        }
    }

    protected override void SwitchCells(Cell newCell)
    {
        currentCell.CellType = CellType.Basic;
        foreach(Cell neigbor in currentCell.GetNeighbors())
        {
            neigbor.CellType = CellType.Basic;
        }
        foreach(Cell neighbor in newCell.GetNeighbors())
        {
            neighbor.CellType = CellType.Obstacle;
        }
        currentCell = newCell;
        newCell.CellType = CellType.Obstacle;
    }
}
