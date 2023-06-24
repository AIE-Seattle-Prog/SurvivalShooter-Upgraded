using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using TNRD.Autohook;

public class EnemyMovement : MonoBehaviour
{
    [field: SerializeField, AutoHook(ReadOnlyWhenFound = true)]
    public NavMeshAgent Nav { get; private set; }               // Reference to the nav mesh agent.
    [field: SerializeField, AutoHook(ReadOnlyWhenFound = true)]
    public Animator Anim { get; private set; }

    public void SetDestination(Vector3 newDestination)
    {
        Nav.SetDestination(newDestination);
    }

    private void OnEnable()
    {
        Nav.enabled = true;
    }

    private void FixedUpdate()
    {
        Anim.SetFloat("Speed", Nav.velocity.magnitude);
    }

    private void OnDisable()
    {
        Nav.enabled = false;
    }
}