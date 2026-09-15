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

    [SerializeField] private GameObject practiceCanvas;
    [SerializeField] private PlanetList planetList; // 인스펙터에서 씬의 PlanetList 오브젝트 연결

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
        if (!TrySelectPlanetForRun())
            return;
        //robbyCanvas.SetActive(false);
        // loadingCanvas.SetActive(false);
        //settingsCanvas.SetActive(false);

        Invoke("DelayedLoading", 1f);
    }
    private bool TrySelectPlanetForRun()
    {
        if (GameRoot.Instance == null)
        {
            Debug.LogError(
                "[LobbyButton] GameRoot.Instance가 없습니다. " +
                "BootStrapScene부터 실행했는지 확인하세요."
            );
            return false;
        }

        GameSessionManager session = GameRoot.Instance.Session;

        if (session == null)
        {
            Debug.LogError("[LobbyButton] GameSessionManager가 없습니다.");
            return false;
        }

        if (planetList == null)
        {
            Debug.LogError("[LobbyButton] PlanetList 참조가 비어있습니다.");
            return false;
        }

        int planetId = planetList.CallingCurrentIndex() + 1; // 0-based -> 1-based

        session.StartPlanetRun(planetId, 4, "LobbyScene");

        return true;
    }
    void DelayedLoading()
    {
        SceneManager.LoadScene("MinigameLoad");
    }

    public void tutorialButtonClick()
    {
        practiceCanvas.SetActive(true);
    }

    public void exitButtonClick()
    {
        practiceCanvas.SetActive(false);
    }


    //public void settingsButtonClick()
    //{
    //    robbyCanvas.SetActive(false);
    //    loadingCanvas.SetActive(false);
    //    settingsCanvas.SetActive(true);
    //}

}
