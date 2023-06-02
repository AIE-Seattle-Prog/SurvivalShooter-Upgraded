using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class PlayerManagerSystem
{
    public static int PlayerCount { get; private set; }
    private static List<GameObject> PlayerObjects = new();

    public static PlayerManager Instance { get; private set; }
    public static PlayerInputManager PlayerInputs => Instance.PlayerInputs;

    private static AsyncOperationHandle<GameObject> loadSystemInstanceOperation;
    
    public static GameObject GetPlayer(int playerIndex) => PlayerObjects[playerIndex];

    //
    // Engine Lifecycle
    //

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnApplicationInit()
    {
        loadSystemInstanceOperation = new();
        Instance = null;
        PlayerCount = 0;
        PlayerObjects = new();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnApplicationFirstSceneLoad()
    {
        loadSystemInstanceOperation = Addressables.LoadAssetAsync<GameObject>("PlayerManager");
        loadSystemInstanceOperation.Completed += HandlePlayerManagerSystemReady;
    }

    private static void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        // spawn and possess new objects for all players
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

            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            OnActiveSceneChanged(new Scene(), SceneManager.GetActiveScene());
        }
        else
        {
            Debug.LogError("Failed to initialize PlayerManager system");
        }
    }

    private static void HandlePlayerJoinedEvent(PlayerInput arg0)
    {
        ++PlayerCount;
        PlayerObjects.Add(arg0.gameObject);

        // TODO - players won't always need/have a group camera. hand this off to
        // an object that can reside in the scene.
        GameStateManager.Instance.CameraGroup.AddMember(arg0.transform, 1.0f, 1.0f);

        Debug.Log("Player joined!");
    }

    private static void HandlePlayerLeftEvent(PlayerInput arg0)
    {
        PlayerObjects.Remove(arg0.gameObject);
        --PlayerCount;

        // TODO - players won't always need/have a group camera. hand this off to
        // an object that can reside in the scene.
        GameStateManager.Instance.CameraGroup.RemoveMember(arg0.transform);

        Debug.Log("Player left!");
    }
}
