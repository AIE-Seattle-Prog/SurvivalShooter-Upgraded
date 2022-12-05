using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
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

    public enum Menus
    {
        None,
        HowToPlay,
        Settings
    }
    public Menus CurrentMenu { get; private set; }
    private GameObject lastShownMenu;

    public void ShowMenu(Menus menu)
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

    public void QuitGame()
    {
        Application.Quit();
    }
}
