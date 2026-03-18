using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

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

    [Header("페이드 타겟")]
    public GameObject fadeTarget;
    [SerializeField] private TextMeshProUGUI planetText;

    private Image fadeImage;
    private CanvasGroup planetTextCanvasGroup;

    private Vector3 targetPosition;
    private int currentPanelIndex = 0;
    private bool isAutoMoving = false;

    void Start()
    {
        upDownMove = planetMove.GetComponent<UpDownMove>();
        fadeImage = fadeTarget.GetComponent<Image>();
        planetList = planetListObj.GetComponent<PlanetList>();

        if (planetText != null)
        {
            Color c = planetText.color;
            c.a = 1f;
            planetText.color = c;
        }

        if (panels.Length > 0)
            targetPosition = transform.position;
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
                StartCoroutine(AutoMoveToNextPanelAfterDelay());
        }
    }

    private IEnumerator AutoMoveToNextPanelAfterDelay()
    {
        isAutoMoving = true;
        yield return new WaitForSeconds(autoMoveDelay);

        if (currentPanelIndex == 1 && panels.Length > 2)
            MoveToPanel(2);
    }

    public void planetButtonClick()
    {
        selectedPlanetIndex = planetList.CallingCurrentIndex();

        if (planetText != null)
        {
            planetText.DOKill();

            Color c = planetText.color;
            planetText.color = c;
        }
        /*
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        if (clickedButton != null)
        {
            Transform child = clickedButton.transform.Find("planetResource");
            if (child != null)
            {
                RectTransform rect = child.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.DOAnchorPos(rect.anchoredPosition + new Vector2(0, -350f), 1f)
                        .SetEase(Ease.InOutQuad);
                }
            }
        }
        */
        StartCoroutine(PlanetSelectionSequence());
    }

    public void MovingCamera(int index)
    {
        if (index < 0 || index >= panels.Length) return;

        Vector3 targetPos = new Vector3(transform.position.x, panels[index].position.y, transform.position.z);
        transform.DOMove(targetPos, 1f).SetEase(Ease.InOutQuad);
        targetPosition = targetPos;
    }

    private IEnumerator PlanetSelectionSequence()
    {
        if (planetText != null)
        {
            planetText.DOKill();

            Color c = planetText.color;
            planetText.color = c;

            planetText.DOFade(0f, 0.15f);
        }

        yield return new WaitForSeconds(1.5f);

        fadeTarget.SetActive(true);
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0f);
        fadeImage.DOFade(1f, 1f);

        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene("MinigameLoad");
    }
}