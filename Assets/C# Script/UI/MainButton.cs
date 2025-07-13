using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainButton : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;   // ������ ī�޶� �巡���Ͽ� �Ҵ�
    [SerializeField] private float moveAmount = 10f;   // ī�޶� �̵��� Y �Ÿ�
    [SerializeField] private float moveDuration = 2f;

    [SerializeField] private Button newGame;
    [SerializeField] private Button loadGame;


    public void newGameClick()
    {
        if (mainCamera == null)
        {
            return;
        }

        // ���� ��ġ �������� Y���� ����
        Vector3 targetPos = mainCamera.transform.position + Vector3.up * moveAmount;

        mainCamera.transform.DOMoveY(targetPos.y, moveDuration)
            .SetEase(Ease.InOutQuad);
    }

    public void newGamePanelClick()
    {
        //newGameScene.SetActive(false);
    }
}
