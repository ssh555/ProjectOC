using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Ð´Ò»¸ö±³ÅÑÎ¥±³×æ×ÚµÄUpdate²âÊÔ½Å±¾
public class TempNavmeshAgent : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform target; 
        
    void Update()
    {
        agent.SetDestination(target.position);
    }
}
