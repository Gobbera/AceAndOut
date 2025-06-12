using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private string levelName;
    [SerializeField] private GameObject panelInitMenu;
    [SerializeField] private GameObject panelOptions;
    [SerializeField] private GameObject panelTutorial;
    public void Play()
    {
        SceneManager.LoadScene(levelName);
    }
    public void OpenOptions()
    {
        panelInitMenu.SetActive(false);
        panelOptions.SetActive(true);
    }
    public void CloseOptions()
    {
        panelOptions.SetActive(false);
        panelInitMenu.SetActive(true);
    }
    public void OpenTutorial()
    {
        panelInitMenu.SetActive(false);
        panelTutorial.SetActive(true);
    }
    public void CloseTutorial()
    {
        panelTutorial.SetActive(false);
        panelInitMenu.SetActive(true);
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
