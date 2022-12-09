using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Round Config", menuName = "Survival Shooter/Enemy/Enemy Round Config", order = 0)]
public class EnemyRoundConfig : ScriptableObject
{
    public int numberOfEnemies;
    public int MaxRollValue => maxBreakpointValue;
    
    public List<EnemyChance> enemyChances = new List<EnemyChance>();

    private List<EnemyChance> enemyBreakpoints = new List<EnemyChance>();
    private int maxBreakpointValue = -1;


    public GameObject GetNextEnemy()
    {
        return GetNextEnemy(Random.Range(0, maxBreakpointValue));
    }

    public GameObject GetNextEnemy(int roll)
    {
        int actualRoll = roll % maxBreakpointValue;

        foreach(var bp in enemyBreakpoints)
        {
            if(actualRoll < bp.weight)
            {
                return bp.enemyPrefab;
            }
        }

        return null;
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

    private void Awake()
    {
        BuildEnemyBreakpoints();
    }
}

[System.Serializable]
public struct EnemyChance
{
    public GameObject enemyPrefab;
    public int weight;
}