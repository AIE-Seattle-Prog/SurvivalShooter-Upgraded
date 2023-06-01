using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameStateManager : MonoBehaviour
{
    [Header("Game State")]
    public float startDelay = 1.0f;
    public AudioClipMetadata inProgressChime;

    public float gameOverDelay = 1.0f;
    public AudioClipMetadata gameoverChime;

    public float restartDelay = 3.0f;
    
    public AudioSource gameStateAudioSource;
    
    private float transitionDelay = 0.0f;
    public float TransitionDelay => transitionDelay;

    private int currentSceneBuildIndex;
    
    public enum GameState
    {
        None,
        Warmup,
        InProgress,
        End
    }
    public GameState CurrentGameState { get; private set; }
    
    public UnityEvent<GameState> OnGameStateChanged; 
    
    public static GameStateManager Instance { get; private set; }

    [Header("Game Settings")]
    public int enemiesPerRound = 30;
    public EnemyRoundConfig[] rounds;
    public int CurrentRound { get; private set; } = -1;

    [field: Header("Sub-Managers")]
    [field: SerializeField]
    public EnemyManager enemyManager { get; private set; }
    [field: SerializeField]
    public Animator hudAnimator { get; private set; }       // Reference to the animator component.

    public void RestartLevel ()
    {
        // Reload the level that is currently loaded.
        SceneManager.LoadScene (currentSceneBuildIndex);
    }

    private void ToGameState(GameState newState)
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
                // turn off all of the spawners
                enemyManager.enabled = false;
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
                ++CurrentRound;
                enemyManager.SetSpawnerConfig(rounds[CurrentRound]);
                enemyManager.ResetSpawnCounts();
                enemyManager.enabled = true;

                transitionDelay = gameOverDelay;
                Debug.Log($"Round {CurrentRound + 1}, starting!");
                if(inProgressChime != null) { gameStateAudioSource.PlayOneShot(inProgressChime); }
                break;
            case GameState.End:
                // tell the animator the game is over.
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

    private void Start()
    {
        currentSceneBuildIndex = SceneManager.GetActiveScene().buildIndex;
        //PlayerManagerSystem.Instance.
        ToGameState(GameState.Warmup);
    }

    private void Update()
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
                bool allPlayersDead = true;

                for(int i = 0; i < PlayerManagerSystem.PlayerCount; ++i)
                {
                    var health = PlayerManagerSystem.GetPlayer(i).GetComponent<PlayerHealth>();
                    if(health.currentHealth > 0)
                    {
                        allPlayersDead = false;
                        break;
                    }
                }

                if (allPlayersDead)
                {
                    transitionDelay -= Time.deltaTime;
                    if (transitionDelay <= 0.0f)
                    {
                        nextState = GameState.End;
                    }
                }
                else if (enemyManager.IsEnemyQuotaMet && enemyManager.EnemiesRemaining <= 0)
                {
                    if (CurrentRound + 1 < rounds.Length)
                    {
                        Debug.Log("Taking a break before the next wave.");
                        nextState = GameState.Warmup;
                    }
                    else
                    {
                        transitionDelay -= Time.deltaTime;
                        if (transitionDelay <= 0.0f)
                        {
                            nextState = GameState.End;
                        }
                    }
                }
                break;
            case GameState.End:
                transitionDelay -= Time.deltaTime;
                if (transitionDelay <= 0.0f)
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

}