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

    [Header("패널 위치들")]
    public Transform[] panels;

    [Header("카메라 이동 속도")]
    public float smoothSpeed = 5f;

    [Header("Panel 2에서 자동 이동까지 대기 시간")]
    public float autoMoveDelay = 3f;

    [Header("선택된 플래닛 오브젝트")]
    public GameObject selectedPlanet;
    public static int selectedPlanetIndex;

    [Header("페이드 타겟 (CanvasGroup)")]
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

    // 매개변수 없이 실행 가능한 버튼용 함수
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
                // 예: 100만큼 아래로 이동 (anchoredPosition 기준)
                RectTransform rect = child.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.DOAnchorPos(rect.anchoredPosition + new Vector2(0, -350f), 1f).SetEase(Ease.InOutQuad);
                }
            }
        }

        MovingCamera(1);

        // 2. 연출 코루틴 시작
        StartCoroutine(PlanetSelectionSequence());
    }

    public void MovingCamera(int index)
    {
        if (index < 0 || index >= panels.Length) return;

        Vector3 targetPos = new Vector3(transform.position.x, panels[index].position.y, transform.position.z);
        transform.DOMove(targetPos, 1f).SetEase(Ease.InOutQuad);

        // Lerp 대상도 바꿔줌
        targetPosition = targetPos;
    }


    private IEnumerator PlanetSelectionSequence()
    {
        yield return new WaitForSeconds(2f);

        fadeTarget.SetActive(true);
        // 알파값 0 → 1 (1초간)
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0f);
        fadeImage.DOFade(1f, 1f);

        yield return new WaitForSeconds(1f); // (페이드 완료 + 대기 포함)

        SceneManager.LoadScene("MinigameLoad");
    }
}
