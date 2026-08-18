using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class CameraScrollController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private GameObject planetListObj;

    private PlanetList planetList;

    [Header("패널 위치들")]
    [SerializeField]
    private Transform[] panels;

    [Header("카메라 이동")]
    [SerializeField]
    private float smoothSpeed = 5f;

    [Header("Panel 2 자동 이동")]
    [SerializeField]
    private float autoMoveDelay = 3f;

    [Header("선택된 행성")]
    public static int selectedPlanetIndex;

    [Header("UI")]
    [SerializeField]
    private TextMeshProUGUI planetText;

    [Header("Scene")]
    [SerializeField]
    private string minigameLoadSceneName =
        "MinigameLoad";

    [Tooltip("행성 선택 후 로딩창을 띄울 때까지의 연출 시간")]
    [SerializeField]
    private float selectionDelay = 1.5f;

    private Vector3 targetPosition;

    private int currentPanelIndex;

    private bool isAutoMoving;
    private bool isSelecting;

    private void Start()
    {
        if (planetListObj != null)
        {
            planetList =
                planetListObj.GetComponent<PlanetList>();
        }

        if (planetText != null)
        {
            Color color = planetText.color;
            color.a = 1f;
            planetText.color = color;
        }

        targetPosition =
            transform.position;
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            Time.deltaTime * smoothSpeed
        );
    }

    public void MoveToPanel(int index)
    {
        if (index < 0 ||
            index >= panels.Length)
        {
            return;
        }

        currentPanelIndex = index;

        Vector3 panelPosition =
            panels[index].position;

        targetPosition =
            new Vector3(
                transform.position.x,
                panelPosition.y,
                transform.position.z
            );

        if (index == 1 &&
            !isAutoMoving)
        {
            StartCoroutine(
                AutoMoveToNextPanelAfterDelay()
            );
        }
    }

    private IEnumerator
        AutoMoveToNextPanelAfterDelay()
    {
        isAutoMoving = true;

        yield return new WaitForSeconds(
            autoMoveDelay
        );

        if (currentPanelIndex == 1 &&
            panels.Length > 2)
        {
            MoveToPanel(2);
        }
    }

    /// <summary>
    /// 행성 선택 버튼의 OnClick에 연결한다.
    /// </summary>
    public void planetButtonClick()
    {
        if (isSelecting)
            return;

        if (planetList == null)
        {
            Debug.LogError(
                "[CameraScrollController] " +
                "PlanetList가 없습니다."
            );

            return;
        }

        selectedPlanetIndex =
            planetList.CallingCurrentIndex();

        StartCoroutine(
            PlanetSelectionSequence()
        );
    }

    public void MovingCamera(int index)
    {
        if (index < 0 ||
            index >= panels.Length)
        {
            return;
        }

        Vector3 target =
            new Vector3(
                transform.position.x,
                panels[index].position.y,
                transform.position.z
            );

        transform
            .DOMove(target, 1f)
            .SetEase(Ease.InOutQuad);

        targetPosition = target;
    }

    private IEnumerator
        PlanetSelectionSequence()
    {
        isSelecting = true;

        if (planetText != null)
        {
            planetText.DOKill();

            planetText
                .DOFade(0f, 0.15f)
                .SetEase(Ease.Linear);
        }

        yield return new WaitForSeconds(
            Mathf.Max(0f, selectionDelay)
        );

        if (GameRoot.Instance == null ||
            GameRoot.Instance.SceneFlow == null)
        {
            Debug.LogError(
                "[CameraScrollController] " +
                "GameRoot 또는 SceneFlowManager가 없습니다. " +
                "BootStrapScene부터 실행했는지 확인하세요."
            );

            isSelecting = false;

            if (planetText != null)
            {
                planetText
                    .DOFade(1f, 0.15f)
                    .SetEase(Ease.Linear);
            }

            yield break;
        }

        // 전역 로딩창 표시
        // → 최소 로딩 시간 대기
        // → MinigameLoad 씬 활성화
        GameRoot.Instance.SceneFlow.LoadScene(
            minigameLoadSceneName
        );
    }
}