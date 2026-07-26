using DG.Tweening;
using TMPro;
using UnityEngine;

public class MainButton : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera mainCamera;

    [Tooltip("화면 입력 후 카메라가 위로 이동할 거리")]
    [SerializeField] private float moveAmount = 10f;

    [Tooltip("카메라 이동 시간")]
    [SerializeField] private float moveDuration = 2f;

    [Header("Blink Text")]
    [Tooltip("깜빡일 안내 텍스트")]
    [SerializeField] private TMP_Text blinkingText;

    [Tooltip("한 번 어두워졌다가 다시 밝아지는 전체 시간")]
    [SerializeField] private float blinkCycleDuration = 2f;

    [Range(0f, 1f)]
    [SerializeField] private float minimumTextAlpha = 0.15f;

    [Header("Scene Transition")]
    [SerializeField] private string lobbySceneName = "LobbyScene";

    [Tooltip("카메라 이동 시작 후 로딩창을 띄울 때까지의 시간")]
    [SerializeField] private float loadingStartDelay = 0.3f;

    private Tween cameraTween;
    private Tween blinkTween;
    private Tween loadingDelayTween;

    private bool isTransitioning;

    private void Start()
    {
        StartBlinkAnimation();
    }

    private void Update()
    {
        if (isTransitioning)
            return;

        // 모바일 터치
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
                StartLobbyTransition();

            return;
        }

        // PC 마우스 클릭
        if (Input.GetMouseButtonDown(0))
            StartLobbyTransition();
    }

    private void StartBlinkAnimation()
    {
        if (blinkingText == null)
            return;

        blinkTween?.Kill();

        Color startColor = blinkingText.color;
        startColor.a = 1f;
        blinkingText.color = startColor;

        float halfDuration =
            Mathf.Max(0.02f, blinkCycleDuration * 0.5f);

        blinkTween = DOTween.Sequence()
            .Append(
                blinkingText
                    .DOFade(minimumTextAlpha, halfDuration)
                    .SetEase(Ease.InOutSine)
            )
            .Append(
                blinkingText
                    .DOFade(1f, halfDuration)
                    .SetEase(Ease.InOutSine)
            )
            .SetLoops(-1)
            .SetUpdate(true);
    }

    private void StartLobbyTransition()
    {
        if (isTransitioning)
            return;

        isTransitioning = true;

        // 입력 안내 텍스트 정지
        blinkTween?.Kill();
        blinkTween = null;

        if (blinkingText != null)
            blinkingText.DOFade(0f, 0.2f).SetUpdate(true);

        // 카메라 위쪽 이동
        if (mainCamera != null)
        {
            float targetY =
                mainCamera.transform.position.y + moveAmount;

            cameraTween = mainCamera.transform
                .DOMoveY(targetY, moveDuration)
                .SetEase(Ease.InOutQuad)
                .SetUpdate(true);
        }

        // 카메라 이동을 잠시 보여준 뒤 전역 로딩창 호출
        loadingDelayTween = DOVirtual.DelayedCall(
            Mathf.Max(0f, loadingStartDelay),
            LoadLobbyScene,
            true
        );
    }

    private void LoadLobbyScene()
    {
        if (GameRoot.Instance == null ||
            GameRoot.Instance.SceneFlow == null)
        {
            Debug.LogError(
                "[MainButton] GameRoot 또는 SceneFlow가 없습니다. " +
                "게임을 BootStrapScene부터 실행했는지 확인하세요."
            );

            isTransitioning = false;
            StartBlinkAnimation();
            return;
        }

        // GameSceneFlowManager가 로딩창 표시,
        // 최소 로딩 시간, 페이드, 씬 전환을 모두 담당한다.
        GameRoot.Instance.SceneFlow.LoadScene(lobbySceneName);
    }

    private void OnDestroy()
    {
        blinkTween?.Kill();
        cameraTween?.Kill();
        loadingDelayTween?.Kill();
    }
}