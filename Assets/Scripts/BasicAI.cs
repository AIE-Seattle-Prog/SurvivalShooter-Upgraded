using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BasicAI : MonoBehaviour
{
    [SerializeField] NavMeshAgent Agent;

    bool InPursuit = false;

    private void Start()
    {
        if(!Agent)
        {
            Agent = GetComponent<NavMeshAgent>();
        }
    }

    public void PursueTarget(GameObject Target)
    {
        Agent.SetDestination(Target.transform.position);

        InPursuit = true;

        Agent.isStopped = false;
    }

    public void StopPursuit()
    {
        Agent.isStopped = true;

        InPursuit = false;
    }

    public void InvestigateNoise(Vector3 Pos)
    {
        if(!InPursuit)
        {
            Agent.SetDestination(Pos);

            Agent.isStopped = false;
        }
    }
}