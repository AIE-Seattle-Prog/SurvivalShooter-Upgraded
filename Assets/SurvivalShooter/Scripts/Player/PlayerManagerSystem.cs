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
    private static AsyncOperationHandle<GameObject> loadSystemInstanceOperation;

    public static int PlayerCount { get; private set; }
    private static List<GameObject> PlayerObjects = new();

    public static PlayerManager Instance { get; private set; }
    public static PlayerInputManager PlayerInputs => Instance.PlayerInputs;

    public static GameObject GetPlayer(int playerIndex) => PlayerObjects[playerIndex];

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void OnApplicationInit() { }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnApplicationFirstSceneLoad()
    {
        loadSystemInstanceOperation = Addressables.LoadAssetAsync<GameObject>("PlayerManager");
        loadSystemInstanceOperation.Completed += Handle_PlayerManagerSystemReady;
    }

    private static void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        // spawn all players
    }

    private static void Handle_PlayerManagerSystemReady(AsyncOperationHandle<GameObject> operation)
    {
        if (operation.Status == AsyncOperationStatus.Succeeded)
        {
            var go = operation.Result;
            var playerInstance = UnityEngine.Object.Instantiate(go);
            Instance = playerInstance.GetComponent<PlayerManager>();
            UnityEngine.Object.DontDestroyOnLoad(Instance.gameObject);

            PlayerInputs.onPlayerJoined += Handle_PlayerJoinedEvent;
            PlayerInputs.onPlayerLeft += Handle_PlayerLeftEvent;

            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            OnActiveSceneChanged(new Scene(), SceneManager.GetActiveScene());
        }
        else
        {
            Debug.LogError("Failed to initialize PlayerManager system");
        }
    }

    private static void Handle_PlayerJoinedEvent(PlayerInput arg0)
    {
        ++PlayerCount;
        PlayerObjects.Add(arg0.gameObject);

        Debug.Log("Player joined!");
    }

    private static void Handle_PlayerLeftEvent(PlayerInput arg0)
    {
        PlayerObjects.Remove(arg0.gameObject);
        --PlayerCount;

        Debug.Log("Player left!");
    }
}
