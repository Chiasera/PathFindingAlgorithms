using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private Grid grid;
    [SerializeField]
    private GameObject agentPrefab;
    [SerializeField]
    private GameObject chairPrefab;
    public static Cell goalCell;
    public static List<Cell> availableCells = new List<Cell>();
    public static List<AgentAI> humans;
    public static List<AgentAI> chairs;

    // Start is called before the first frame update
    void Start()
    {
        if(availableCells == null)
        {
            availableCells = new List<Cell>();
        }
        if(humans == null)
        {
            humans = new List<AgentAI>();
        }
        if(chairs == null)
        {
            chairs = new List<AgentAI>();
        }
        humans.Clear();
        chairs.Clear();
        availableCells.Clear();
        grid = FindObjectOfType<Grid>();
        grid.GenerateGrid(GridType.Connected8);
        //Set random goal onto the map
        int randGoalPosition = Random.Range(0, availableCells.Count - 1);
        goalCell = availableCells[randGoalPosition];
        goalCell.CellType = CellType.Goal; //randomly choose a goal
        availableCells.Remove(goalCell);
        for (int i = 0; i < 20; i++)
        {
            SpawnAgent();
        }
        for(int i = 0; i < 20; i++)
        {
            SpawnChair();
        }
    }

    public void SpawnAgent()
    {
        int randAgentPosition = Random.Range(0, availableCells.Count - 1); 
        HumanAI agent = Instantiate(agentPrefab).GetComponent<HumanAI>();
        agent.transform.position = availableCells[randAgentPosition].transform.position + Vector3.up * 1.5f;
        agent.SetCurrentCell(availableCells[randAgentPosition]);
        agent.Activate(3000, GameManager.goalCell);
        availableCells.Remove(availableCells[randAgentPosition]);
        humans.Add(agent);
    }

    //assuming the chair will be spawn after the player
    public void SpawnChair()
    {
        int randomChairPosition = Random.Range(0, availableCells.Count - 1);
        ChairAI chair = Instantiate(chairPrefab).GetComponent<ChairAI>();
        chair.transform.position = availableCells[randomChairPosition].transform.position + Vector3.up * 1.5f;
        chair.Activate(humans);
        chair.SetCurrentCell(availableCells[randomChairPosition]);
        //availableCells[randomChairPosition].CellType = CellType.Obstacle;
        availableCells.Remove(availableCells[randomChairPosition]);
        chairs.Add(chair);
    }
}
