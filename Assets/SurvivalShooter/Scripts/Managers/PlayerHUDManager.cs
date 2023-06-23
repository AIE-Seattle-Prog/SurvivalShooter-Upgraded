using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDManager : MonoBehaviour, IMapCheck
{
    [HeaderCentered("UI Elements")]
    public Text enemyCount;
    public string formatter = "00";

    public Animator hudAnimator;

    [SerializeField]
    private RoundHUDManager roundHud;
    [SerializeField]
    private HealthWidget[] healthWidgets;

    public bool Check()
    {
        if (enemyCount == null) { return false; }
        if (hudAnimator == null) { return false; }
        if (roundHud == null) { return false; }

        return true;
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

    private void HandleEnemyCountUpdated(int arg0)
    {
        enemyCount.text = GameStateManager.Instance.EnemyManager.EnemiesRemaining.ToString(formatter);
    }

    private void HandlePlayerJoined(PlayerController arg0)
    {
        HealthWidget targetWidget = null;
        foreach(var curHealth in healthWidgets)
        {
            if(curHealth.OwningPlayer == null)
            {
                targetWidget = curHealth;
                break;
            }
        }

        Debug.Assert(targetWidget != null, "Could not find a health widget for this player!");

        targetWidget.BindToPlayer(arg0);
    }

    private void OnEnable()
    {
        PlayerManagerSystem.OnPlayerJoined.AddListener(HandlePlayerJoined);
        GameStateManager.Instance.EnemyManager.OnEnemyCountChanged.AddListener(HandleEnemyCountUpdated);
        GameStateManager.Instance.OnGameStateChanged.AddListener(HandleGameStateChanged);
    }


    private void OnDisable()
    {
        PlayerManagerSystem.OnPlayerJoined.RemoveListener(HandlePlayerJoined);
        GameStateManager.Instance.EnemyManager.OnEnemyCountChanged.RemoveListener(HandleEnemyCountUpdated);
        GameStateManager.Instance.OnGameStateChanged.RemoveListener(HandleGameStateChanged);
    }
}
