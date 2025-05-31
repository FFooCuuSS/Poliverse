using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyButton : MonoBehaviour
{
    [SerializeField] private Button planetSelect;
    [SerializeField] private Button leftPlanet;
    [SerializeField] private Button rightPlanet;
    [SerializeField] private Button settings;

    [SerializeField] private GameObject robbyCanvas;
    [SerializeField] private GameObject loadingCanvas;
    [SerializeField] private GameObject settingsCanvas;


    private void Start()
    {
        robbyCanvas.SetActive(true);
        loadingCanvas.SetActive(false);
        settingsCanvas.SetActive(false);
    }

    public void planetButtonClick()
    {
        robbyCanvas.SetActive(false);
        loadingCanvas.SetActive(true);
        settingsCanvas.SetActive(false);
    }

    public void settingsButtonClick()
    {
        robbyCanvas.SetActive(false);
        loadingCanvas.SetActive(false);
        settingsCanvas.SetActive(true);
    }

}
