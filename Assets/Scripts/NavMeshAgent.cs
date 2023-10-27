using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class NavMeshAgent : MonoBehaviour
{
    private Stack<Cell> path;
    private Cell currentCell;
    private Cell nextCell;
    [SerializeField]
    [Range(1, 1000)]
    private float speed;
    private Cell[] cellsArray;
    private Rigidbody rb;
    private bool isAsleep = true;
    [SerializeField]
    [Range(0, 50)]
    private float rotationSpeed;
    public List<Vector3> splinePoints;
    public Cell CurrentCell { get { return currentCell; } set { currentCell = value; } }
    private A_StarSearch pathSearch;

    public void SetTarget(Cell cell)
    {
        pathSearch = new A_StarSearch(currentCell, cell);
        path = pathSearch.StartSearch();
        cellsArray = path.ToArray();
        nextCell = path.Pop();
        isAsleep = false;
        splinePoints = pathSearch.SplineControlPoints;
    }

    private void OnDrawGizmos()
    {
        if (path != null && path.Count > 0)
        {
            for (int i = 0; i < cellsArray.Length - 1; i++)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(cellsArray[i].transform.position + Vector3.up, cellsArray[i + 1].transform.position + Vector3.up);
            }
        }
        if(splinePoints != null && splinePoints.Count > 0)
        {
            foreach (var pos in splinePoints)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(new Vector3(pos.x, 1, pos.z), 0.5f);
            }
        }     
    }

    public async void Activate(int ms)
    {
        await Task.Delay(ms);
        if (this.isActiveAndEnabled)
        {
            rb = GetComponent<Rigidbody>();
            SetTarget(GameManager.goalCell);
        }        
    }

    public void SetCurrentCell(Cell cell)
    {
        currentCell = cell;
    }

    private void Update()
    {
        if (!isAsleep)
        {
            FollowPath();
        }
    }

    public void FollowPath()
    {
        Vector3 direction = new Vector3(nextCell.transform.position.x, 0, nextCell.transform.position.z)
            - new Vector3(transform.position.x, 0, transform.position.z);
        if (direction.magnitude < 0.5f)
        {
            if (path.Count > 0)
            {
                nextCell = path.Pop();
            }
        }       
        if (nextCell == GameManager.goalCell)
        {
            rb.velocity = direction * speed * Time.deltaTime;
        }
        else
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            rb.velocity = direction.normalized * speed * Time.deltaTime;
        }
    }
}
