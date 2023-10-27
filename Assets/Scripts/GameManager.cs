using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private Grid grid;
    [SerializeField]
    private GameObject agentPrefab;
    public static Cell goalCell;
    // Start is called before the first frame update
    void Start()
    {
        grid = FindObjectOfType<Grid>();
        /*------------ For  debug purposes, try adding some obstacles ------------*/
        for(int i = 0; i < grid.gridSize_N * 2; i++)
        {
            Vector2Int randomObstacle = new Vector2Int(Random.Range(0, grid.gridSize_N - 1), Random.Range(0, grid.gridSize_N - 1));
            grid.Cells[randomObstacle].CellType = CellType.Obstacle;
        }
        /*-----------------------------------------------------------------------*/
        //Set random goal onto the map
        Vector2Int randGoalPos = new Vector2Int(Random.Range(0, grid.gridSize_N - 1), Random.Range(0, grid.gridSize_N - 1));
        goalCell = grid.Cells[randGoalPos];
        goalCell.CellType = CellType.Goal; //randomly choose a goal
        SpawnAgent();
    }

    public async void OnWaitSpawnAgent(int ms)
    {
        await Task.Delay(ms);
        SpawnAgent();
    }

    public void SpawnAgent()
    {
        Vector2Int randAgentPos = new Vector2Int(Random.Range(0, grid.gridSize_N - 1), Random.Range(0, grid.gridSize_N - 1));
        NavMeshAgent agent = Instantiate(agentPrefab).GetComponent<NavMeshAgent>();
        agent.transform.position = new Vector3(randAgentPos.x, 2, randAgentPos.y);
        agent.SetCurrentCell(grid.Cells[randAgentPos]);
        agent.Activate(5000);
    }
}
