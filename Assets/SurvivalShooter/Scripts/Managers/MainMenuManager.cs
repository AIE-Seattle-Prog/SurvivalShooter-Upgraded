using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Controls")]
    [SerializeField]
    private Button playButton;
    [SerializeField]
    private Button howToPlayButton;
    [SerializeField]
    private Button settingsButton;
    [SerializeField]
    private Button quitGameButton;

    [SerializeField]
    private GameObject howToPlayPanel;
    [SerializeField]
    private GameObject settingsPanel;

    private CancellationTokenSource canvasCancellationSource;
    private CancellationTokenSource transitionCancellationSource;

    [Header("UI Animations")]
    [SerializeField]
    private Animation animRunner;
    [SerializeField]
    private AnimationClip startClip;

    public enum Menus
    {
        None,
        HowToPlay,
        Settings
    }
    public Menus CurrentMenu { get; private set; }
    private GameObject lastShownMenu;

    public async void ShowMenu(Menus menu)
    {
        // do nothing if we are trying to show the currently shown menu
        if(menu == CurrentMenu) {return;}
        
        // hide last menu if any
        if (lastShownMenu != null)
        {
            lastShownMenu.SetActive(false);
        }

        GameObject newMenu = null;
        
        switch (menu)
        {
            case Menus.None:
                newMenu = null;
                break;
            case Menus.Settings:
                newMenu = settingsPanel;
                break;
            case Menus.HowToPlay:
                newMenu = howToPlayPanel;
                break;
        }

        if (newMenu != null)
        {
            newMenu.SetActive(true);
        }

        lastShownMenu = newMenu;
        CurrentMenu = menu;

        // transition if group is available
        if (transitionCancellationSource.Token != null)
        {
            transitionCancellationSource.Cancel();
            transitionCancellationSource.Dispose();
            transitionCancellationSource = new CancellationTokenSource();
        }

        if (newMenu.TryGetComponent<CanvasGroup>(out var newGroup))
        {
            newGroup.alpha = 0.0f;
            await DoAlphaTransitionAsync(newMenu.GetComponent<CanvasGroup>(), 1.0f, 0.3f, transitionCancellationSource.Token);
        }
    }

    public void ShowMenu(int menuEnumId)
    {
        ShowMenu((Menus)(menuEnumId));
    }

    public void DismissCurrentMenu()
    {
        ShowMenu(Menus.None);
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    private async Task DoAlphaTransitionAsync(CanvasGroup group, float targetAlpha, float duration, CancellationToken token)
    {
        try
        {
            float startAlpha = group.alpha;
            float timer = 0.0f;
            while (timer < duration)
            {
                float progress = timer / duration;

                group.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);

                timer += Time.deltaTime;

                await UniTask.Yield(token); // yield return null in a coroutine
            }

            group.alpha = targetAlpha;
        }
        catch(OperationCanceledException)
        {
            Debug.Log("Alpha transition cancelled");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private async void PresentMainMenu(CancellationToken token)
    {
        try
        {
            animRunner.AddClip(startClip, "Start");
            animRunner.Play("Start");

            playButton.interactable = false;
            howToPlayButton.interactable = false;
            settingsButton.interactable = false;
            quitGameButton.interactable = false;

            Debug.Log("Wait for animation...");

            await UniTask.WaitUntil(() => !animRunner.isPlaying, cancellationToken: canvasCancellationSource.Token);

            Debug.Log("Animation finished! Enable menu...");

            await UniTask.Delay(100, cancellationToken: canvasCancellationSource.Token);
            playButton.interactable = true;
            await UniTask.Delay(100, cancellationToken: canvasCancellationSource.Token);
            howToPlayButton.interactable = true;
            await UniTask.Delay(100, cancellationToken: canvasCancellationSource.Token);
            settingsButton.interactable = true;
            await UniTask.Delay(100, cancellationToken: canvasCancellationSource.Token);
            quitGameButton.interactable = true;
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Main menu presentation cancelled");
        }
    }
    
    private void Start()
    {
        PresentMainMenu(canvasCancellationSource.Token);
    }

    private void OnEnable()
    {
        transitionCancellationSource = new CancellationTokenSource();
        canvasCancellationSource = new CancellationTokenSource();
    }

    private void OnDisable()
    {
        transitionCancellationSource.Cancel();
        transitionCancellationSource.Dispose();

        canvasCancellationSource.Cancel();
        canvasCancellationSource.Dispose();
    }
}
