using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFOV : MonoBehaviour
{
    public float radius;
    [Range(0,360)]
    public float angle;

    public GameObject player;

    public LayerMask targetMask;
    public LayerMask obstructionMask;

    public bool seePlayer;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(FOVRoutine());

    }

    private void Update()
    {
        if (seePlayer)
        {
            Debug.Log("WE HAVE SIGHT");
        }
    }
    private IEnumerator FOVRoutine()
    {
        float delay = 0.2f;
        WaitForSeconds wait = new WaitForSeconds(delay);

        while (true) 
        {
            yield return wait;
            FOVCheck();
        }
    }

    private void FOVCheck()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, targetMask);

        if(rangeChecks.Length != 0) 
        {
            Transform target = rangeChecks[0].transform;
            Vector3 directToTarget = (target.position - transform.position).normalized;

            if(Vector3.Angle(transform.forward, directToTarget) < angle / 2)
            {
                float distanceFromTarget = Vector3.Distance(transform.position, target.position);

                if(!Physics.Raycast(transform.position, directToTarget, distanceFromTarget, obstructionMask)) 
                {
                    seePlayer = true;
                }
                else
                {
                    seePlayer = false;
                }
            }
            else
            {
                seePlayer = false;
            }
        }
        else if(seePlayer) 
        {
            seePlayer = false;
        }
    }
}
