using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameConfirmDialog : MonoBehaviour
{
    [Header("Dialog")]
    [SerializeField]
    private GameObject dialogRoot;

    [SerializeField]
    private TMP_Text messageText;

    [Header("Buttons")]
    [SerializeField]
    private Button yesButton;

    [SerializeField]
    private Button noButton;

    [SerializeField]
    private TMP_Text yesButtonText;

    [SerializeField]
    private TMP_Text noButtonText;

    [Header("Default Text")]
    [SerializeField, TextArea(2, 4)]
    private string defaultMessage =
        "정말 진행하시겠습니까?";

    [SerializeField]
    private string defaultYesText = "네";

    [SerializeField]
    private string defaultNoText = "아니오";

    public bool IsOpen =>
        dialogRoot != null &&
        dialogRoot.activeSelf;

    private Action currentOnYes;
    private Action currentOnNo;

    private bool listenersRegistered;

    private void Awake()
    {
        RegisterButtonListeners();
        HideImmediate();
    }

    private void Update()
    {
        if (!IsOpen)
            return;

        // PC의 Esc와 모바일의 뒤로 가기 버튼
        if (Input.GetKeyDown(KeyCode.Escape))
            SelectNo();
    }

    private void OnDestroy()
    {
        UnregisterButtonListeners();
    }

    /// <summary>
    /// 기본 메시지와 기본 버튼 문구로 확인창을 표시한다.
    /// </summary>
    public void ShowDefault(
        Action onYes = null,
        Action onNo = null)
    {
        Show(
            defaultMessage,
            defaultYesText,
            defaultNoText,
            onYes,
            onNo
        );
    }

    /// <summary>
    /// 메시지만 변경하고 버튼 문구는
    /// 기본값인 네/아니오를 사용한다.
    /// </summary>
    public void Show(
        string message,
        Action onYes = null,
        Action onNo = null)
    {
        Show(
            message,
            defaultYesText,
            defaultNoText,
            onYes,
            onNo
        );
    }

    /// <summary>
    /// 메시지와 버튼 문구를 모두 지정한다.
    /// </summary>
    public void Show(
        string message,
        string yesText,
        string noText,
        Action onYes = null,
        Action onNo = null)
    {
        RegisterButtonListeners();

        if (dialogRoot == null)
        {
            Debug.LogError(
                "[ConfirmDialog] Dialog Root가 없습니다."
            );

            return;
        }

        // 이전 확인창의 콜백을 새 요청으로 교체한다.
        currentOnYes = onYes;
        currentOnNo = onNo;

        if (messageText != null)
        {
            messageText.text =
                string.IsNullOrWhiteSpace(message)
                    ? defaultMessage
                    : message;
        }

        if (yesButtonText != null)
        {
            yesButtonText.text =
                string.IsNullOrWhiteSpace(yesText)
                    ? defaultYesText
                    : yesText;
        }

        if (noButtonText != null)
        {
            noButtonText.text =
                string.IsNullOrWhiteSpace(noText)
                    ? defaultNoText
                    : noText;
        }

        if (yesButton != null)
            yesButton.interactable = true;

        if (noButton != null)
            noButton.interactable = true;

        dialogRoot.transform.SetAsLastSibling();
        dialogRoot.SetActive(true);
    }

    /// <summary>
    /// 예 버튼과 동일하게 동작한다.
    /// 코드 또는 UnityEvent에서도 호출할 수 있다.
    /// </summary>
    public void SelectYes()
    {
        if (!IsOpen)
            return;

        Action callback =
            currentOnYes;

        // 콜백이 씬을 바꾸기 전에 확인창부터 닫는다.
        HideImmediate();

        callback?.Invoke();
    }

    /// <summary>
    /// 아니오 버튼 및 모바일 뒤로 가기와 동일하게 동작한다.
    /// </summary>
    public void SelectNo()
    {
        if (!IsOpen)
            return;

        Action callback =
            currentOnNo;

        HideImmediate();

        callback?.Invoke();
    }

    /// <summary>
    /// 콜백을 실행하지 않고 즉시 닫는다.
    /// </summary>
    public void HideImmediate()
    {
        currentOnYes = null;
        currentOnNo = null;

        if (dialogRoot != null)
            dialogRoot.SetActive(false);
    }

    private void RegisterButtonListeners()
    {
        if (listenersRegistered)
            return;

        if (yesButton != null)
        {
            yesButton.onClick.AddListener(
                SelectYes
            );
        }

        if (noButton != null)
        {
            noButton.onClick.AddListener(
                SelectNo
            );
        }

        listenersRegistered = true;
    }

    private void UnregisterButtonListeners()
    {
        if (!listenersRegistered)
            return;

        if (yesButton != null)
        {
            yesButton.onClick.RemoveListener(
                SelectYes
            );
        }

        if (noButton != null)
        {
            noButton.onClick.RemoveListener(
                SelectNo
            );
        }

        listenersRegistered = false;
    }
}