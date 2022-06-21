using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameStateManager : MonoBehaviour
{
    public PlayerHealth playerHealth;       // Reference to the player's health.
    
    public Animator hudAnimator;       // Reference to the animator component.

    public float startDelay = 1.0f;
    public float gameOverDelay = 1.0f;
    public float restartDelay = 3.0f;
    
    public AudioClip inProgressChime;
    public AudioClip gameoverChime;
    public AudioSource gameStateAudioSource;
    
    private float transitionDelay = 0.0f;
    public float TransitionDelay => transitionDelay;

    private int sceneBuildIndex;
    
    public enum GameState
    {
        None,
        Warmup,
        InProgress,
        End
    }
    public GameState CurrentGameState { get; private set; }
    
    [SerializeField]
    private EnemyManager[] enemyManagers;

    public UnityEvent<GameState> OnGameStateChanged; 
    
    public static GameStateManager Instance { get; private set; }

    void ToGameState(GameState newState)
    {
        // exit current state - clean-up
        Debug.Log("Exiting game state: " + CurrentGameState);
        switch (CurrentGameState)
        {
            case GameState.None:
                // left blank intentionally
                break;
            case GameState.Warmup:
                break;
            case GameState.InProgress:
                // turn on all of the spawners
                foreach (var spawner in enemyManagers) { spawner.enabled = false; }
                break;
            case GameState.End:
                break;
            default:
                Debug.LogError("Unhandled exit from game state: " + CurrentGameState);
                break;
        }
        
        // enter new state - clean-up
        Debug.Log("Entering game state: " + newState);
        switch (newState)
        {
            case GameState.None:
                // left blank intentionally
                break;
            case GameState.Warmup:
                transitionDelay = startDelay;
                break;
            case GameState.InProgress:
                // turn on all of the spawners
                foreach (var spawner in enemyManagers) { spawner.enabled = true; }
                transitionDelay = gameOverDelay;
                if(inProgressChime != null) { gameStateAudioSource.PlayOneShot(inProgressChime); }
                break;
            case GameState.End:
                // ... tell the animator the game is over.
                hudAnimator.SetTrigger ("GameOver");
                transitionDelay = restartDelay;
                if(gameoverChime != null) { gameStateAudioSource.PlayOneShot(gameoverChime); }
                break;
            default:
                Debug.LogError("Unhandled entry to game state: " + CurrentGameState);
                break;
        }

        // update bookkeeping
        CurrentGameState = newState;
        
        // fire events
        OnGameStateChanged.Invoke(CurrentGameState);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Debug.Log("GameStateManager instance registered.");
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Duplicate GameStateManager detected on gameObject! Self-destructing...", gameObject);
            Destroy(this);
            return;
        }
    }

    void Start()
    {
        sceneBuildIndex = SceneManager.GetActiveScene().buildIndex;
        ToGameState(GameState.Warmup);
    }

    void Update ()
    {
        var nextState = CurrentGameState;
        switch (CurrentGameState)
        {
            case GameState.Warmup:
                transitionDelay -= Time.deltaTime;
                if (transitionDelay <= 0.0f)
                {
                    nextState = GameState.InProgress;
                }
                break;
            case GameState.InProgress:
                if (playerHealth.currentHealth <= 0)
                {
                    transitionDelay -= Time.deltaTime;
                    if (transitionDelay <= 0.0f)
                    {
                        nextState = GameState.End;
                    }
                }
                break;
            case GameState.End:
                transitionDelay -= Time.deltaTime;
                if(transitionDelay <= 0.0f)
                {
                    RestartLevel();
                }
                break;
            default:
                break;
        }

        // is a transition needed?
        if (nextState != CurrentGameState)
        {
            ToGameState(nextState);
        }
    }
    
    public void RestartLevel ()
    {
        // Reload the level that is currently loaded.
        SceneManager.LoadScene (sceneBuildIndex);
    }
}