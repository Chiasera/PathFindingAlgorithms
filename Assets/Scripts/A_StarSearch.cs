using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class A_StarSearch
{

    private Grid grid;
    private GridCell startCell;
    private GridCell targetCell;
    public A_StarSearch(Grid grid, GridCell startCell, GridCell targetCell)
    {
        this.grid = grid;
        this.startCell = startCell;
        this.targetCell = targetCell;
        startCell.G_Cost = 0;
        startCell.F_Cost = startCell.H_Star(targetCell);
    }

    public List<GridCell> StartSearch()
    {
        //initialize cost to 0;
        float currentCost = 0;
        List<GridCell> openList = new List<GridCell>();
        List<GridCell> closedList = new List<GridCell>();
        openList.Add(startCell);
        //while open list is not empty
        while (openList.Count > 0)
        {
            GridCell currentCell = GetLowestCostCell(openList, ref currentCost);
            openList.Remove(currentCell);
            closedList.Add(currentCell);

            // If the target node is reached, reconstruct the path from start to target
            if (currentCell == targetCell)
            {
                return ReconstructPath(targetCell);
            }

            foreach (GridCell neighbor in currentCell.GetNeighbors())
            {
                //if already visited that cell
                if (closedList.Contains(neighbor))
                {
                    //skip, since already previously evaluated;
                    continue;
                }
                //Calculate tentative gCost for neighbor
                float tentative_gCost = currentCost + neighbor.TravelCost;
                if (!openList.Contains(neighbor))
                {
                    openList.Add(neighbor);
                } else if (tentative_gCost >= neighbor.G_Cost)
                {
                    //previous path was cheaper, skip
                    continue;
                }
                // This path is the best until now, store it
                neighbor.CameFrom = currentCell;
                neighbor.G_Cost = tentative_gCost;
                neighbor.F_Cost = neighbor.G_Cost + neighbor.H_Star(targetCell);
            }
        }
        Debug.LogError("COULD NOT FIND A PATH, TERMINATING...");
        return null;
    }

    private List<GridCell> ReconstructPath(GridCell goalCell)
    {
        List<GridCell> path = new List<GridCell>();
        while(goalCell.CameFrom != null)
        {
            path.Add(goalCell);
            goalCell = goalCell.CameFrom;
        }
        path.Add(startCell);
        return path;
    }

    private GridCell GetLowestCostCell(List<GridCell> cells, ref float currentCost)
    {
        GridCell bestCell = null;
        float f = Mathf.Infinity;
        foreach(GridCell cell in cells)
        {
            //f(s) = g(s) + h(s) = cost_so_far + heuristic where cost_so_far
            if(cell.F_Cost < f)
            {
                f = cell.F_Cost;
                bestCell = cell;
            }
        }
        currentCost += bestCell.TravelCost;
        return bestCell;
    }
}
