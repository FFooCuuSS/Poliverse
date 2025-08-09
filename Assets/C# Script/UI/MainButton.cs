using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainButton : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;   // 씬에서 카메라를 드래그하여 할당
    [SerializeField] private float moveAmount = 10f;   // 카메라가 이동할 Y 거리
    [SerializeField] private float moveDuration = 2f;

    [SerializeField] private Button newGame;
    [SerializeField] private Button loadGame;


    public void newGameClick()
    {
        if (mainCamera == null)
        {
            return;
        }

        // 현재 위치 기준으로 Y값만 증가
        Vector3 targetPos = mainCamera.transform.position + Vector3.up * moveAmount;

        mainCamera.transform.DOMoveY(targetPos.y, moveDuration)
            .SetEase(Ease.InOutQuad);
    }

    public void newGamePanelClick()
    {
        //newGameScene.SetActive(false);
    }
}
