using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//дһ������Υ�����ڵ�Update���Խű�
public class TempNavmeshAgent : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform target; 
        
    void Update()
    {
        agent.SetDestination(target.position);
    }
}
