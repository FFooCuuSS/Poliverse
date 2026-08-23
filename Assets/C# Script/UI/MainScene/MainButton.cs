using DG.Tweening;
using TMPro;
using UnityEngine;

public class MainButton : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField]
    private Camera mainCamera;

    [Tooltip("화면 입력 후 카메라가 위로 이동할 거리")]
    [SerializeField]
    private float moveAmount = 10f;

    [Tooltip("카메라 이동 시간")]
    [SerializeField]
    private float moveDuration = 2f;

    [Header("Blink Text")]
    [Tooltip("화면을 터치하여 시작 텍스트")]
    [SerializeField]
    private TMP_Text blinkingText;

    [Tooltip("한 번 어두워졌다가 다시 밝아지는 전체 시간")]
    [SerializeField]
    private float blinkCycleDuration = 2f;

    [Range(0f, 1f)]
    [SerializeField]
    private float minimumTextAlpha = 0.15f;

    [Header("Scene Transition")]
    [SerializeField]
    private string lobbySceneName = "LobbyScene";

    [Tooltip("카메라 이동 시작 후 로딩창을 띄울 때까지의 시간")]
    [SerializeField]
    private float loadingStartDelay = 0.3f;

    private Tween cameraTween;
    private Tween blinkTween;
    private Tween loadingDelayTween;

    private bool isTransitioning;
    private bool inputEnabled;

    private void Awake()
    {
        HideStartPrompt();
    }

    private void Start()
    {
        HideStartPrompt();
    }

    private void Update()
    {
        if (!inputEnabled ||
            isTransitioning)
        {
            return;
        }

        if (Input.touchCount > 0)
        {
            Touch touch =
                Input.GetTouch(0);

            if (touch.phase ==
                TouchPhase.Began)
            {
                StartLobbyTransition();
            }

            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            StartLobbyTransition();
        }
    }

    public void ShowStartPrompt()
    {
        if (isTransitioning)
            return;

        inputEnabled = true;

        // 핵심:
        // Hierarchy에서 비활성화되어 있어도
        // 로그인 완료 후 다시 켜준다.
        if (blinkingText != null)
        {
            blinkingText
                .gameObject
                .SetActive(true);
        }

        StartBlinkAnimation();

        Debug.Log(
            "[MainButton] 화면 터치 시작 활성화."
        );
    }

    public void HideStartPrompt()
    {
        inputEnabled = false;

        blinkTween?.Kill();
        blinkTween = null;

        if (blinkingText == null)
            return;

        // GameObject 자체는 꺼도 되고,
        // 로그인 완료 때 ShowStartPrompt에서 다시 켠다.
        blinkingText
            .gameObject
            .SetActive(false);
    }

    private void StartBlinkAnimation()
    {
        if (blinkingText == null)
        {
            Debug.LogWarning(
                "[MainButton] Blinking Text가 없습니다."
            );

            return;
        }

        blinkingText
            .gameObject
            .SetActive(true);

        blinkTween?.Kill();

        Color color =
            blinkingText.color;

        color.a = 1f;

        blinkingText.color =
            color;

        float halfDuration =
            Mathf.Max(
                0.02f,
                blinkCycleDuration * 0.5f
            );

        blinkTween =
            DOTween.Sequence()
                .Append(
                    blinkingText
                        .DOFade(
                            minimumTextAlpha,
                            halfDuration
                        )
                        .SetEase(
                            Ease.InOutSine
                        )
                )
                .Append(
                    blinkingText
                        .DOFade(
                            1f,
                            halfDuration
                        )
                        .SetEase(
                            Ease.InOutSine
                        )
                )
                .SetLoops(-1)
                .SetUpdate(true);
    }

    private void StartLobbyTransition()
    {
        if (!inputEnabled ||
            isTransitioning)
        {
            return;
        }

        isTransitioning = true;
        inputEnabled = false;

        blinkTween?.Kill();
        blinkTween = null;

        if (blinkingText != null)
        {
            blinkingText
                .DOFade(
                    0f,
                    0.2f
                )
                .SetUpdate(true);
        }

        if (mainCamera != null)
        {
            float targetY =
                mainCamera
                    .transform
                    .position
                    .y +
                moveAmount;

            cameraTween =
                mainCamera
                    .transform
                    .DOMoveY(
                        targetY,
                        moveDuration
                    )
                    .SetEase(
                        Ease.InOutQuad
                    )
                    .SetUpdate(true);
        }

        loadingDelayTween =
            DOVirtual.DelayedCall(
                Mathf.Max(
                    0f,
                    loadingStartDelay
                ),
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
                "BootstrapScene부터 실행했는지 확인하세요."
            );

            isTransitioning = false;

            ShowStartPrompt();

            return;
        }

        GameRoot.Instance
            .SceneFlow
            .LoadScene(
                lobbySceneName
            );
    }

    private void OnDestroy()
    {
        blinkTween?.Kill();
        cameraTween?.Kill();
        loadingDelayTween?.Kill();
    }
}