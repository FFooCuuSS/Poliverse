using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CameraScrollController : MonoBehaviour
{
    public GameObject planetMove;
    public GameObject planetListObj;
    private UpDownMove upDownMove;
    private PlanetList planetList;

    [Header("�г� ��ġ��")]
    public Transform[] panels;

    [Header("ī�޶� �̵� �ӵ�")]
    public float smoothSpeed = 5f;

    [Header("Panel 2���� �ڵ� �̵����� ��� �ð�")]
    public float autoMoveDelay = 3f;

    [Header("���õ� �÷��� ������Ʈ")]
    public GameObject selectedPlanet;
    public static int selectedPlanetIndex;

    [Header("���̵� Ÿ�� (CanvasGroup)")]
    public GameObject fadeTarget;

    private Image fadeImage;

    private Vector3 targetPosition;
    private int currentPanelIndex = 0;
    private bool isAutoMoving = false;

    void Start()
    {
        upDownMove = planetMove.GetComponent<UpDownMove>();
        fadeImage = fadeTarget.GetComponent<Image>();
        planetList = planetListObj.GetComponent<PlanetList>();

        if (panels.Length > 0)
        {
            targetPosition = transform.position;
        }
    }

    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
    }

    public void MoveToPanel(int index)
    {
        if (index >= 0 && index < panels.Length)
        {
            currentPanelIndex = index;
            Vector3 panelPos = panels[index].position;
            targetPosition = new Vector3(transform.position.x, panelPos.y, transform.position.z);

            if (index == 1 && !isAutoMoving)
            {
                StartCoroutine(AutoMoveToNextPanelAfterDelay());
            }
        }
    }

    private IEnumerator AutoMoveToNextPanelAfterDelay()
    {
        isAutoMoving = true;
        yield return new WaitForSeconds(autoMoveDelay);

        if (currentPanelIndex == 1 && panels.Length > 2)
        {
            MoveToPanel(2);
        }
    }

    // �Ű����� ���� ���� ������ ��ư�� �Լ�
    public void planetButtonClick()
    {
        upDownMove.StopMoving();
        selectedPlanetIndex = planetList.CallingCurrentIndex();
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        if (clickedButton != null)
        {
            Transform child = clickedButton.transform.Find("planetResource");
            if (child != null)
            {
                // ��: 100��ŭ �Ʒ��� �̵� (anchoredPosition ����)
                RectTransform rect = child.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.DOAnchorPos(rect.anchoredPosition + new Vector2(0, -350f), 1f).SetEase(Ease.InOutQuad);
                }
            }
        }

        MovingCamera(1);

        // 2. ���� �ڷ�ƾ ����
        StartCoroutine(PlanetSelectionSequence());
    }

    public void MovingCamera(int index)
    {
        if (index < 0 || index >= panels.Length) return;

        Vector3 targetPos = new Vector3(transform.position.x, panels[index].position.y, transform.position.z);
        transform.DOMove(targetPos, 1f).SetEase(Ease.InOutQuad);

        // Lerp ��� �ٲ���
        targetPosition = targetPos;
    }


    private IEnumerator PlanetSelectionSequence()
    {
        yield return new WaitForSeconds(2f);

        fadeTarget.SetActive(true);
        // ���İ� 0 �� 1 (1�ʰ�)
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0f);
        fadeImage.DOFade(1f, 1f);

        yield return new WaitForSeconds(1f); // (���̵� �Ϸ� + ��� ����)

        SceneManager.LoadScene("MinigameLoad");
    }
}
