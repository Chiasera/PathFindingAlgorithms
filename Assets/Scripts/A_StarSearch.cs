using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

//seperate the search state from the cell itself
public class CellSearchState
{
    public float G_Cost { get; set; } = Mathf.Infinity;
    public float F_Cost { get; set; } = Mathf.Infinity;
    public Cell CameFrom { get; set; }

    public CellSearchState()
    {
        G_Cost = Mathf.Infinity;
        F_Cost = Mathf.Infinity;
        CameFrom = null;
    }
}

public class A_StarSearch
{
    private Cell startCell;
    private Cell targetCell;
    private Dictionary<Cell, CellSearchState> searchStates;
    private Dictionary<Vector3, Vector3> splineControlPoints;
    public Dictionary<Vector3, Vector3> SplineKnotsPositions { get { return splineControlPoints; } }
    public A_StarSearch(Cell startCell, Cell targetCell)
    {
        this.startCell = startCell;
        this.targetCell = targetCell;

        searchStates = new Dictionary<Cell, CellSearchState>();
        CellSearchState startState = new CellSearchState
        {
            G_Cost = 0,
            F_Cost = startCell.Heuristic(targetCell)
        };
        searchStates[startCell] = startState;
    }

    public Stack<Cell> StartSearch()
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
                UnityEngine.Debug.Log("Path finding time: " + sw.Elapsed.TotalMilliseconds + "ms");
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
                    CellSearchState cellSearchState = new CellSearchState
                    {
                        G_Cost = 0,
                        F_Cost = neighbor.Heuristic(targetCell)
                    };
                    searchStates[neighbor] = cellSearchState;
                }
                else if (tentative_gCost >= searchStates[neighbor].G_Cost)
                {
                    //previous path was cheaper, skip
                    continue;
                }
                // This path is the best until now, store it
                searchStates[neighbor].CameFrom = currentCell;
                searchStates[neighbor].G_Cost = tentative_gCost;
                searchStates[neighbor].F_Cost = searchStates[neighbor].G_Cost + neighbor.Heuristic(targetCell);
            }
        }
        UnityEngine.Debug.LogError("COULD NOT FIND A PATH, TERMINATING...");
        return null;
    }

    /// <summary>
    /// Reconstructs the path from the goal cell back to the start cell.
    /// </summary>
    /// <param name="goalCell">The goal cell from which to start the reconstruction.</param>
    /// <returns>A stack of cells representing the path.</returns>
    private Stack<Cell> ReconstructPath(Cell goalCell)
    {
        // Initialize directions to zero.
        Vector3 currentDirection = Vector3.zero;
        Vector3 nextDirection = Vector3.zero;

        // Ensure splineControlPoints is initialized.
        if (splineControlPoints == null)
        {
            splineControlPoints = new Dictionary<Vector3, Vector3>();
        }
        splineControlPoints.Clear();

        // Initialize the path stack.
        Stack<Cell> path = new Stack<Cell>();

        // Iterate back from the goal cell using the CameFrom references.
        while (searchStates[goalCell].CameFrom != null)
        {
            currentDirection = nextDirection;
            nextDirection = searchStates[goalCell].CameFrom.transform.position - goalCell.transform.position;

            // Add a control point if the direction changes.
            if (nextDirection.normalized != currentDirection.normalized)
            {
                splineControlPoints.Add(goalCell.transform.position, nextDirection);
            }

            // Push the current cell onto the path stack.
            path.Push(goalCell);

            // Move to the previous cell in the path.
            goalCell = searchStates[goalCell].CameFrom;
        }
        splineControlPoints.Add(startCell.transform.position, nextDirection);
        // Return the reconstructed path.
        return path;
    }


    private Cell GetLowestCostCell(List<Cell> cells, ref float currentCost)
    {
        Cell bestCell = null;
        float f = Mathf.Infinity;
        foreach (Cell cell in cells)
        {
            var state = searchStates[cell];
            if (state.F_Cost < f)
            {
                f = state.F_Cost;
                bestCell = cell;
            }
        }
        currentCost += bestCell.TravelCost;
        return bestCell;
    }
}
