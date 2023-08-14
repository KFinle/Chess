using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MenuManager : MonoBehaviour
{
    [SerializeField] private Server server;
    [SerializeField] private Client client;

    public GameObject MainMenu;
    public GameObject CreditsMenu;
    public GameObject PlayMenu;
    public GameObject NetplayMenu;

    /// <summary>
    /// Enter play menu 
    /// </summary>
    public void PlayButton()
    {
        MainMenu.SetActive(false);

        PlayMenu.SetActive(true);
    }

    /// <summary>
    /// Enter credits screen
    /// </summary>
    public void CreditsButton()
    {
        // Show Credits Menu
        MainMenu.SetActive(false);
        CreditsMenu.SetActive(true);
    }

    /// <summary>
    /// Enter main menu 
    /// </summary>
    public void MainMenuButton()
    {
        // Show Main Menu
        MainMenu.SetActive(true);
        CreditsMenu.SetActive(false);
        OptionsMenu.SetActive(false);
        PlayMenu.SetActive(false);
        NetplayMenu.SetActive(false);
    }

    /// <summary>
    /// Quit game
    /// </summary>
    public void QuitButton()
    {
        Application.Quit();
    }

    public void PlayLocalGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LocalSoloGame");
    }

    /// <summary>
    /// Enter online game menu
    /// </summary>
    public void PlayOnlineGame()
    {
        NetplayMenu.SetActive(true);
        PlayMenu.SetActive(false);
    }

    /// <summary>
    /// Enter AI game menu
    /// </summary>
    public void PlayCPUGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LocalAiGame");
    }


}