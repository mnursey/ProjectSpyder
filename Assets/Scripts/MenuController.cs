using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject waitMenu;
    public GameObject endMenu;
    public GameObject optionsMenu;
    public GameObject tutorialMenu;
    public GameObject connectionErrorMenu;
    public GameObject creditsMenu;

    public GameObject currentMenu;

    public AudioSource clickSoundEffect;

    public ClientGameRunner cgr;

    public Text connectionErrorText;
    public Text waitingText;
    public Text endingText;

    public static MenuController Instance;

    public void Awake()
    {
       if(Instance == null)
       {
            Instance = this;
       } else
       {
            Debug.LogError("More than one Menu Controller");
       }
    }

    public void Start()
    {
        GoToMainMenu();
    }

    public void CloseAllMenus()
    {
        mainMenu.SetActive(false);
        waitMenu.SetActive(false);
        endMenu.SetActive(false);
        optionsMenu.SetActive(false);
        tutorialMenu.SetActive(false);
        connectionErrorMenu.SetActive(false);
        creditsMenu.SetActive(false);
    }

    public void Play()
    {
        if (clickSoundEffect != null) clickSoundEffect.Play();
        CloseAllMenus();
        cgr.ConnectToGame();
    }

    public void GoToMenu(GameObject menu, bool click = true)
    {
        if (currentMenu != null)
            currentMenu.SetActive(false);

        menu.SetActive(true);
        currentMenu = menu;

        if (click && clickSoundEffect != null) clickSoundEffect.Play();
    }

    public void GoToMainMenu()
    {
        GoToMenu(mainMenu);
    }

    public void GoToWaitMenu()
    {
        GoToMenu(waitMenu);
    }

    public void GoToEndMenu()
    {
        GoToMenu(endMenu);
    }

    public void GoToOptionsMenu()
    {
        GoToMenu(optionsMenu);
    }

    public void GoToTutorialMenu()
    {
        GoToMenu(tutorialMenu);
    }
    public void GoToConnectionErrorMenu()
    {
        GoToMenu(connectionErrorMenu);
    }

    public void GoToCreditsMenu()
    {
        GoToMenu(creditsMenu);
    }

    public void CloseMenu()
    {
        currentMenu.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SetConnectionErrorReason(string reason)
    {
        connectionErrorText.text = reason;
    }
    public void SetWaitingText(string txt)
    {
        waitingText.text = txt;
    }
    public void SetEndingText(string txt)
    {
        endingText.text = txt;
    }

    public static MenuController GetMC()
    {
        return GameObject.Find("MenuController").GetComponent<MenuController>();
    }
}