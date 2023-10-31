using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class HumanAI : NavMeshAgent
{
    private bool detectCollision = true;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Chair") 
            || collision.gameObject.CompareTag("Human"))
        {
            if (detectCollision)
            {
                SetTarget(GameManager.goalCell);
                Debug.Log("RECALCULATING PATH!!!");
                OnPathRecalculateWait();
            }
        }
    }

    private async void OnPathRecalculateWait()
    {
        if (detectCollision)
        {
            detectCollision = false;
            await Task.Delay(500);
            detectCollision = true;
        }
    }


    private void OnDestroy()
    {
        GameManager.humans.Remove(this);
        currentCell.CellType = CellType.Basic;
    }
}
