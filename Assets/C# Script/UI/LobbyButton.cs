using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class LobbyButton : MonoBehaviour
{
    [SerializeField] private Button planetSelect;
    [SerializeField] private Button leftPlanet;
    [SerializeField] private Button rightPlanet;
    //[SerializeField] private Button settings;

    //[SerializeField] private GameObject robbyCanvas;
    //[SerializeField] private GameObject loadingCanvas;
    //[SerializeField] private GameObject settingsCanvas;


    private void Start()
    {
        //robbyCanvas.SetActive(true);
       // loadingCanvas.SetActive(false);
        //settingsCanvas.SetActive(false);
    }

    public void planetButtonClick()
    {
        //robbyCanvas.SetActive(false);
       // loadingCanvas.SetActive(false);
        //settingsCanvas.SetActive(false);

        Invoke("DelayedLoading", 1f);
    }

    void DelayedLoading()
    {
        SceneManager.LoadScene("MinigameLoad");
    }

    //public void settingsButtonClick()
    //{
    //    robbyCanvas.SetActive(false);
    //    loadingCanvas.SetActive(false);
    //    settingsCanvas.SetActive(true);
    //}

}
