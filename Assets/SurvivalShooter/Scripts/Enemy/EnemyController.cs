using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TNRD.Autohook;

public class EnemyController : MonoBehaviour
{
    [field: SerializeField, AutoHook(ReadOnlyWhenFound = true)]
    public EnemyHealth Health { get; private set; }
    [field: SerializeField, AutoHook(ReadOnlyWhenFound = true)]
    public EnemyMovement Movement { get; private set; }
    [field: SerializeField, AutoHook(ReadOnlyWhenFound = true)]
    public Animator Anim { get; private set; }

    [field: Space, SerializeField]
    public PlayerCharacter TargetPlayer { get; set; }

    private void Update()
    {
        // early exit if no health
        if (Health.currentHealth <= 0) { return; }

        // If the enemy and the player have health left...
        if (TargetPlayer != null && TargetPlayer.health.currentHealth > 0)
        {
            // ... set the destination of the nav mesh agent to the player.
            Movement.SetDestination(TargetPlayer.transform.position);
        }
        // Otherwise, change targets...
        else
        {
            bool hadTarget = TargetPlayer != null;
            PlayerCharacter newTarget = null;

            int rngSelection = Random.Range(0, PlayerManagerSystem.PlayerCount);
            for(int i = 0; i < PlayerManagerSystem.PlayerCount; ++i)
            {
                PlayerCharacter candidate = PlayerManagerSystem.GetPlayer((rngSelection + i) % PlayerManagerSystem.PlayerCount).Character;
                if(candidate.health.currentHealth > 0)
                {
                    newTarget = candidate;
                    break;
                }
            }

            if (newTarget != null)
            {
                TargetPlayer = newTarget;
                Movement.enabled = true;
            }
            else
            {
                // ... disable the nav mesh agent.
                Movement.enabled = false;
            }

            bool hasTarget = newTarget != null;

            // Update animator
            if(hadTarget != hasTarget) { /**/ }
        }
    }
}
