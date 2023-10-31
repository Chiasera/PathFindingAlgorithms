using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
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
    private int maxIterationsPerFrame = 30;
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

    public async Task<Stack<Cell>> StartSearch()
    {
        Stopwatch sw = Stopwatch.StartNew();
        sw.Start(); // Start the stopwatch
        //initialize cost to 0;
        float currentCost = 0;
        List<Cell> openList = new List<Cell>();
        List<Cell> closedList = new List<Cell>();
        openList.Add(startCell);
        //while open list is not empty
        int iterationCount = 0;
        while (openList.Count > 0)
        {
            if(iterationCount > maxIterationsPerFrame)
            {
                //If takes too much time, wait for next frame
                await Task.Yield();
                iterationCount = 0; 

            }
            iterationCount++;
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
            if(startCell == null)
            {
                
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
        int cellDelta = 0;

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
            if(cellDelta % 2 == 0)
            {
                splineControlPoints.Add(goalCell.transform.position, nextDirection);
            }
            // Push the current cell onto the path stack.
            path.Push(goalCell);
            // Move to the previous cell in the path.
            goalCell = searchStates[goalCell].CameFrom;
            cellDelta++;
        }
        splineControlPoints.Add(startCell.transform.position, currentDirection);
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
            //if surrounded by obstacles still allow to go through
            if (state.F_Cost <= f)
            {
                f = state.F_Cost;
                bestCell = cell;
            }
        }
        if(bestCell != null && bestCell.CellType != CellType.Obstacle)
        {
            currentCost += bestCell.TravelCost;
        }
        return bestCell;
    }
}
