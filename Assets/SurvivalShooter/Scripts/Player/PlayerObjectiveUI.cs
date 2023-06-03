using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObjectiveUI : MonoBehaviour
{
    public UnityEngine.UI.Text objectiveLabel;

    public string waitingText = "Waiting for Players...";
    public string warmupText = "Warmup";
    public string inProgressText = "In Progress";
    public string gameoverText = "Game Over";
    
    public Animation animation;
    
    private void Start()
    {
        HandleGameStateChanged(GameStateManager.Instance.CurrentGameState);
        GameStateManager.Instance.OnGameStateChanged.AddListener(HandleGameStateChanged);
    }

    private void FixedUpdate()
    {
        if (GameStateManager.Instance.CurrentGameState == GameStateManager.GameState.Warmup &&
            GameStateManager.Instance.TransitionDelay < 4.0f)
        {
            objectiveLabel.text = $"{warmupText} {GameStateManager.Instance.TransitionDelay:0}";
        }
    }

    private void HandleGameStateChanged(GameStateManager.GameState newState)
    {
        switch (newState)
        {
            case GameStateManager.GameState.WaitingForConnection:
                objectiveLabel.text = waitingText;
                break;
            case GameStateManager.GameState.Warmup:
                objectiveLabel.text = warmupText;
                break;
            case GameStateManager.GameState.InProgress:
                objectiveLabel.text = inProgressText;
                break;
            case GameStateManager.GameState.End:
                objectiveLabel.text = gameoverText;
                break;
        }

        animation.Play();
    }
}
