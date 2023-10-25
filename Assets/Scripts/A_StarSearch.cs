using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class A_StarSearch
{
    private Cell startCell;
    private Cell targetCell;
    public A_StarSearch( Cell startCell, Cell targetCell)
    {
        this.startCell = startCell;
        this.targetCell = targetCell;
        startCell.G_Cost = 0;
        startCell.F_Cost = startCell.Heuristic(targetCell);
    }

    public List<Cell> StartSearch()
    {
        Stopwatch sw = Stopwatch.StartNew();
        sw.Start(); // Start the stopwatch
        //initialize cost to 0;
        float currentCost = 0;
        List<Cell> openList = new List<Cell>();
        List<Cell> closedList = new List<Cell>();
        openList.Add(startCell);
        //while open list is not empty
        while (openList.Count > 0)
        {
            Cell currentCell = GetLowestCostCell(openList, ref currentCost);
            openList.Remove(currentCell);
            closedList.Add(currentCell);

            // If the target node is reached, reconstruct the path from start to target
            if (currentCell == targetCell)
            {
                sw.Stop();
                UnityEngine.Debug.Log(sw.Elapsed);
                return ReconstructPath(targetCell);
            }

            foreach (Cell neighbor in currentCell.GetNeighbors())
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
                neighbor.F_Cost = neighbor.G_Cost + neighbor.Heuristic(targetCell);
            }
        }
        UnityEngine.Debug.LogError("COULD NOT FIND A PATH, TERMINATING...");       
        return null;
    }

    private List<Cell> ReconstructPath(Cell goalCell)
    {
        List<Cell> path = new List<Cell>();
        while(goalCell.CameFrom != null)
        {
            path.Add(goalCell);
            goalCell = goalCell.CameFrom;
        }
        path.Add(startCell);
        return path;
    }

    private Cell GetLowestCostCell(List<Cell> cells, ref float currentCost)
    {
        Cell bestCell = null;
        float f = Mathf.Infinity;
        foreach(Cell cell in cells)
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
