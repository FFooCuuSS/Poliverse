using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainButton : MonoBehaviour
{
    [SerializeField] private Button newGame;
    [SerializeField] private Button loadGame;
    [SerializeField] private Button newGamePanel;

    [SerializeField] private GameObject newGameScene;

    private void Start()
    {
        newGameScene.SetActive(false);
    }

    public void newGameClick()
    {
        newGameScene.SetActive(true);
    }

    public void newGamePanelClick()
    {
        newGameScene.SetActive(false);
    }
}
