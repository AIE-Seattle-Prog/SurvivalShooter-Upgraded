using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDManager : MonoBehaviour
{
    public Text enemyCount;
    public string formatter = "00";

    public Animator hudAnimator;

    [SerializeField]
    private RoundHUDManager roundHud;

    private void OnEnable()
    {
        GameStateManager.Instance.enemyManager.OnEnemyCountChanged.AddListener(HandleEnemyCountUpdated);
        GameStateManager.Instance.OnGameStateChanged.AddListener(HandleGameStateChanged);
    }

    private void HandleGameStateChanged(GameStateManager.GameState arg0)
    {
        if(arg0 == GameStateManager.GameState.InProgress)
        {
            bool isLastRound = GameStateManager.Instance.CurrentRound == GameStateManager.Instance.rounds.Length - 1;
            if (!isLastRound)
            {
                roundHud.SetRoundNumber(GameStateManager.Instance.CurrentRound + 1);
            }
            else
            {
                roundHud.SetRoundNumberAsString("Final Round");
            }
            hudAnimator.SetTrigger("RoundIntro");
        }
    }

    private void OnDisable()
    {
        GameStateManager.Instance.enemyManager.OnEnemyCountChanged.RemoveListener(HandleEnemyCountUpdated);
        GameStateManager.Instance.OnGameStateChanged.RemoveListener(HandleGameStateChanged);
    }

    private void HandleEnemyCountUpdated(int arg0)
    {
        enemyCount.text = GameStateManager.Instance.enemyManager.EnemiesRemaining.ToString(formatter);
    }
}
