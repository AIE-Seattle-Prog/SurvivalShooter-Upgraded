using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Cinemachine;

public class GameStateManager : MonoBehaviour
{
    [Header("Game State")]
    public GameObject PlayerCharacterPrefab;
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
        WaitingForConnection,
        Warmup,
        InProgress,
        End
    }
    public GameState CurrentGameState { get; private set; }
    
    /// <summary>
    /// Called after the game state transitions from one state to another
    /// </summary>
    public UnityEvent<GameState> OnGameStateChanged;
    /// <summary>
    /// Called when a player starts playing in the current match
    /// </summary>
    public UnityEvent<PlayerController> OnPlayerStart;
    /// <summary>
    /// Called when a player quits from the current match
    /// </summary>
    public UnityEvent<PlayerController> OnPlayerQuit;

    public static GameStateManager Instance { get; private set; }

    [Header("Game Settings")]
    public int enemiesPerRound = 30;
    public EnemyRoundConfig[] rounds;
    public int CurrentRound { get; private set; } = -1;

    [field: Header("Sub-Managers")]
    [field: SerializeField]
    public EnemyManager EnemyManager { get; private set; }
    [field: SerializeField]
    public Animator HudAnimator { get; private set; }       // Reference to the animator component.
    [field: SerializeField]
    public CinemachineTargetGroup CameraGroup { get; private set; }

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
            case GameState.WaitingForConnection:
            case GameState.None:
            case GameState.Warmup:
                break;
            case GameState.InProgress:
                // turn off all of the spawners
                EnemyManager.enabled = false;
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
            case GameState.WaitingForConnection:
            case GameState.None:
            case GameState.Warmup:
                transitionDelay = startDelay;
                break;
            case GameState.InProgress:
                ++CurrentRound;
                EnemyManager.SetSpawnerConfig(rounds[CurrentRound]);
                EnemyManager.ResetSpawnCounts();
                EnemyManager.enabled = true;

                transitionDelay = gameOverDelay;
                Debug.Log($"Round {CurrentRound + 1}, starting!");
                if(inProgressChime != null) { gameStateAudioSource.PlayOneShot(inProgressChime); }
                break;
            case GameState.End:
                // tell the animator the game is over.
                HudAnimator.SetTrigger ("GameOver");
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

    private void AddPlayer(PlayerController player)
    {
        var newBody = Instantiate(PlayerCharacterPrefab);
        player.Possess(newBody.GetComponent<PlayerCharacter>());
        CameraGroup.AddMember(newBody.transform, 1.0f, 1.0f);

        OnPlayerStart?.Invoke(player);
    }

    //
    // Framework Handlers
    //

    private void HandleOnPlayerJoined(PlayerController newPlayer)
    {
        AddPlayer(newPlayer);
    }

    //
    // Engine Handlers
    //

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private void HandleGameReset()
    {
        Instance = null;
    }

    //
    // MonoBehaviour Magic
    //

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

        // spawn existing players
        for (int i = 0; i < PlayerManagerSystem.PlayerCount; ++i)
        {
            HandleOnPlayerJoined(PlayerManagerSystem.GetPlayer(i));
        }
        PlayerManagerSystem.OnPlayerJoined.AddListener(HandleOnPlayerJoined);

        ToGameState(GameState.WaitingForConnection);
    }

    private void Update()
    {
        var nextState = CurrentGameState;
        switch (CurrentGameState)
        {
            case GameState.WaitingForConnection:
                if(PlayerManagerSystem.PlayerCount > 0)
                {
                    nextState = GameState.Warmup;
                }
                break;
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
                    var health = PlayerManagerSystem.GetPlayer(i).Character.health;
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
                else if (EnemyManager.IsEnemyQuotaMet && EnemyManager.EnemiesRemaining <= 0)
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