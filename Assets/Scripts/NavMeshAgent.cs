using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class NavMeshAgent : AgentAI
{
    public GameObject splinePrefab;
    protected Stack<Cell> path;
    protected Cell[] cellsArray;
    protected Spline spline;
    public Stack<Cell> Path { get { return path; } }
    protected A_StarSearch pathSearch;
    [Range(1, 20)]
    public float pathCurvature = 1.0f;
    protected bool killCurrentPath = false;
    protected bool changePath = false;
    protected float repellForce = 1.0f;
    private float repellRadius = 4.0f;
    private int forecastDistance = 3;

    public async void SetTarget(Cell cell)
    {       
        killCurrentPath = true; 
        //if already on a path, don't lerp backwards to go to the current cell's center position
        //Instead, lerp towards the next cell on the path and set it as the current cell
        if(spline != null)
        {
            ChooseNextStartPosition(cell);
        }
        pathSearch = new A_StarSearch(currentCell, cell);
        path = await pathSearch.StartSearch();
        cellsArray = path.ToArray();
        if(path.Count > 0)
        {
            nextCell = path.Pop();
        }
        UpdateSpline();
        isAsleep = false;
        await LerpToStartPosition();
        changePath = false;
    }


    private void ChooseNextStartPosition(Cell targetCell)
    {
        currentCell = currentCell.GetNeighbors().OrderBy(neighbor => neighbor.Heuristic(targetCell) * neighbor.TravelCost).First();
    }


    private void AvoidObstacles(List<AgentAI> agents, float repellForce)
    {
        foreach (var obstacle in agents)
        {
            if (isActiveAndEnabled && obstacle != null && obstacle != this)
            {
                //additional logic here hehehe
                if (Vector3.Distance(transform.position, obstacle.transform.position) < repellRadius)
                {
                    for (int i = 1; i < forecastDistance + 1; i++)
                    {
                        float relativeDistance = forecastDistance - (i)/3;
                        int maxIndex = Mathf.FloorToInt(spline.T) + i;
                        if (maxIndex > 0 && maxIndex < spline.Knots.Count - 2)
                        {
                            SplineKnot knot = spline.Knots[Mathf.FloorToInt(spline.T) + i];
                            if (Vector3.Distance(knot.transform.position, obstacle.transform.position) < relativeDistance)
                            {
                                Vector3 directionToObstacle = obstacle.transform.position - knot.transform.position;
                                Vector3 tangent = spline.DerivativeAtSegment(spline.T % Mathf.Max(i, 1), Mathf.FloorToInt(spline.T));        
                                //is the obstacle on the right or the left side of the curve ?
                                Vector3 obstaclePosition = Vector3.Cross(tangent, new Vector3(directionToObstacle.x, 0, directionToObstacle.z));
                                Debug.DrawRay(knot.transform.position, tangent, Color.red);
                                Vector3 perpendicular = Vector3.Cross(tangent, Vector3.up).normalized;
                                knot.transform.position += perpendicular * Time.deltaTime * repellForce * Mathf.Sign(obstaclePosition.y);
                            }
                        }
                    }
                } 
            }
        }
    }

    protected void UpdateSpline()
    {
        if (spline == null)
        {
            spline = Instantiate(splinePrefab).GetComponent<Spline>();

        }   
        spline.Knots.ForEach(knot =>
        {
            Destroy(knot.gameObject);
        });
        spline.Knots.Clear();
        spline.T = 0;
        spline.Create(pathSearch.SplineKnotsPositions);
    }

    private async Task LerpToStartPosition()
    {
        Vector3 direction = spline.CurveInfo.currentPosition - transform.position;
        Quaternion targetRotation;
        while(direction.magnitude > 0.05f)
        {
            transform.position += direction.normalized * Time.deltaTime * movementSpeed;
            direction = spline.CurveInfo.currentPosition - transform.position;
            targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            await Task.Yield();
        }
        killCurrentPath = false;
    }

    protected void OnDrawGizmos()
    {
        if (path != null && path.Count > 0)
        {
            for (int i = 0; i < cellsArray.Length - 1; i++)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(cellsArray[i].transform.position + Vector3.up, cellsArray[i + 1].transform.position + Vector3.up);
            }
        }  
    }

    public virtual async void Activate(int delayMilliseconds, Cell target)
    {
        await Task.Delay(delayMilliseconds);
        if (this.isActiveAndEnabled)
        {
            rb = GetComponent<Rigidbody>();
            SetTarget(target);
        }        
    }

    protected virtual void Update()
    {
        OnCellChange();
        if (!isAsleep && !killCurrentPath)
        {
            FollowPath();
            AvoidObstacles(GameManager.humans, 1.0f);
            AvoidObstacles(GameManager.chairs, 1.0f);
        }
       
        if(Vector3.Distance(transform.position, GameManager.goalCell.transform.position) < 2f)
        {
            Destroy(gameObject);
        }
    }

    public void FollowPath()
    {
        if (spline.SegmentLength > 0)
        {
            //somewhat constant velocity, no acceleration
            spline.T += Time.deltaTime * movementSpeed;
        }
        else
        {
            spline.T += Time.deltaTime;
        }
        transform.position = spline.CurveInfo.currentPosition;
        if (Vector3.Distance(transform.position, nextCell.transform.position ) < 1.5f && Path.Count > 0)
        {
            nextCell = path.Pop();
        }
        // Calculate the target rotation based on the velocity direction
        Quaternion targetRotation = Quaternion.LookRotation(spline.CurveInfo.velocity.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }     
}
