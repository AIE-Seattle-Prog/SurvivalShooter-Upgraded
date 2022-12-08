using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDManager : MonoBehaviour
{
    public Text enemyCount;
    public string formatter = "00";

    private void OnEnable()
    {
        GameStateManager.Instance.enemyManager.OnEnemyCountChanged.AddListener(HandleEnemyCountUpdated);
    }

    private void OnDisable()
    {
        GameStateManager.Instance.enemyManager.OnEnemyCountChanged.RemoveListener(HandleEnemyCountUpdated);
    }

    private void HandleEnemyCountUpdated(int arg0)
    {
        enemyCount.text = GameStateManager.Instance.enemyManager.EnemiesRemaining.ToString(formatter);
    }
}
