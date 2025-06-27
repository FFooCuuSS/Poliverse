using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainButton : MonoBehaviour
{
    [SerializeField] private Button newGame;
    [SerializeField] private Button loadGame;
    [SerializeField] private Button newGamePanel;

    [SerializeField] private GameObject newGameScene;
    [SerializeField] private GameObject tempObject;

    private void Start()
    {
        newGameScene.SetActive(false);
    }

    public void newGameClick()
    {
        //newGameScene.SetActive(true);
        SceneManager.LoadScene("LobbyScene");
    }

    public void newGamePanelClick()
    {
        //newGameScene.SetActive(false);
    }

    public void tempClick()
    {
        tempObject.SetActive(false);
    }
}
