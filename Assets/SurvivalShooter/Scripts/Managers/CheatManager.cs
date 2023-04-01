using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CheatManager
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private void Initialize()
    {
        SceneManager.activeSceneChanged += HandleActiveSceneChanged;
    }

    private void HandleActiveSceneChanged(Scene arg0, Scene arg1)
    {
        InjectActiveScene();
    }

    private void InjectActiveScene()
    {
        
    }
}
