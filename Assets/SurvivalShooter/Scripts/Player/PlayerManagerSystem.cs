using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Events;

public static class PlayerManagerSystem
{
    public static int PlayerCount { get; private set; }
    private static List<PlayerController> PlayerObjects = new();

    public static PlayerManager Instance { get; private set; }
    public static PlayerInputManager PlayerInputs => Instance.PlayerInputs;

    public static PlayerController GetPlayer(int playerIndex) => PlayerObjects[playerIndex];

    /// <summary>
    /// Called when a player joins the session. Players stay "joined", even across scene loads,
    /// until they choose to leave.
    /// </summary>
    public static UnityEvent<PlayerController> OnPlayerJoined { get; private set; }
    /// <summary>
    /// Called when a player leaves the session.
    /// </summary>
    public static UnityEvent<PlayerController> OnPlayerLeft { get; private set; }
    
    private static AsyncOperationHandle<GameObject> loadSystemInstanceOperation;

    //
    // Engine Handlers
    //

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnApplicationInit()
    {
        loadSystemInstanceOperation = new();
        Instance = null;
        PlayerCount = 0;
        PlayerObjects = new();
        
        // reininitialize events
        OnPlayerJoined = new();
        OnPlayerLeft = new();
        Application.quitting -= HandleApplicationQuit;
        Application.quitting += HandleApplicationQuit;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnApplicationFirstSceneLoad()
    {
        loadSystemInstanceOperation = Addressables.LoadAssetAsync<GameObject>("PlayerManager");
        loadSystemInstanceOperation.Completed += HandlePlayerManagerSystemReady;
    }

    //
    // Event Handlers
    //

    private static void HandlePlayerManagerSystemReady(AsyncOperationHandle<GameObject> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            var go = operation.Result;
            var playerInstance = UnityEngine.Object.Instantiate(go);
            Instance = playerInstance.GetComponent<PlayerManager>();
            UnityEngine.Object.DontDestroyOnLoad(Instance.gameObject);

            PlayerInputs.onPlayerJoined += HandlePlayerJoinedEvent;
            PlayerInputs.onPlayerLeft += HandlePlayerLeftEvent;
        }
        else
        {
            Debug.LogError("Failed to initialize PlayerManager system");
        }
    }

    private static void HandlePlayerJoinedEvent(PlayerInput arg0)
    {
        ++PlayerCount;
        PlayerObjects.Add(arg0.GetComponent<PlayerController>());
        UnityEngine.Object.DontDestroyOnLoad(arg0.gameObject);

        // TODO: cache these instead
        OnPlayerJoined.Invoke(arg0.GetComponent<PlayerController>());

        Debug.Log("Player joined!");
    }

    private static void HandlePlayerLeftEvent(PlayerInput arg0)
    {
        PlayerObjects.Remove(arg0.GetComponent<PlayerController>());
        --PlayerCount;
        // No need to destroy, engine will take care of it if needed

        // TODO: access from cache instead
        OnPlayerLeft.Invoke(arg0.GetComponent<PlayerController>());

        Debug.Log("Player left!");
    }

    private static void HandleApplicationQuit()
    {
        while(PlayerObjects.Count > 0)
        {
            UnityEngine.Object.Destroy(PlayerObjects[0].gameObject);
        }
        Debug.Assert(PlayerObjects.Count == 0);
    }
}
