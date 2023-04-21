using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Round Config", menuName = "Survival Shooter/Enemy/Enemy Round Config", order = 0)]
public class EnemyRoundConfig : ScriptableObject
{
    [Tooltip("Number of enemies to spawn in this wave")]
    public int numberOfEnemies;

    [System.Serializable]
    public struct EnemyChance
    {
        public GameObject enemyPrefab;
        public int weight;
    }

    [System.Serializable]
    public struct EnemyPlan
    {
        public GameObject enemy;
        [Tooltip("The index that will guarantee the spawning of this enemy")]
        public int position;
    }

    [Tooltip("A weighted list of enemy types to spawn")]
    public List<EnemyChance> enemyChances = new List<EnemyChance>();
    [Tooltip("A list of specific enemies to spawn in the spawn sequence")]
    public List<EnemyPlan> enemyOverrides = new List<EnemyPlan>();

    public int MaxRollValue => maxBreakpointValue;

    private List<EnemyChance> enemyBreakpoints = new List<EnemyChance>();
    private int maxBreakpointValue = -1;

    public GameObject GetRandomEnemy()
    {
        int actualRoll = Random.Range(0, maxBreakpointValue);

        foreach(var bp in enemyBreakpoints)
        {
            if(actualRoll < bp.weight)
            {
                return bp.enemyPrefab;
            }
        }

        return null;
    }

    public bool GetPlannedEnemy(int spawnIndex, out GameObject enemyPrefab)
    {
        enemyPrefab = null;

        foreach (var eOverride in enemyOverrides)
        {
            if(eOverride.position == spawnIndex) { enemyPrefab = eOverride.enemy; return true; }
            if(eOverride.position > spawnIndex) { return false; }
        }

        return false;
    }

    private void BuildEnemyBreakpoints ()
    {
        List<EnemyChance> breakpoints = new List<EnemyChance>();

        int distance = 0;

        foreach(var chance in enemyChances)
        {
            distance += chance.weight;
            int newBreakpoint = distance;
            breakpoints.Add(new EnemyChance() { enemyPrefab = chance.enemyPrefab, weight = newBreakpoint });
        }
        maxBreakpointValue = distance;

        enemyBreakpoints = breakpoints;
    }

    /*
    private void BuildEnemyPlan()
    {
        List<EnemyPlan> enemySequence = new List<EnemyPlan>();

        // sort by position in spawn sequence
        enemyOverrides.Sort((a, b) => a.position.CompareTo(b.position));
    }
    */

    private void Awake()
    {
        BuildEnemyBreakpoints();
    }

    private void OnValidate()
    {
        // TODO: ensure overrides are sorted
    }
}