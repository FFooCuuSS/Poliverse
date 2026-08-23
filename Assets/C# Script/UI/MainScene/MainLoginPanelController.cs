using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainLoginPanelController : MonoBehaviour
{
    [Header("Login UI")]
    [SerializeField]
    private CanvasGroup loginCanvasGroup;

    [SerializeField]
    private Button googleLoginButton;

    [SerializeField]
    private Button guestContinueButton;

    [SerializeField]
    private TMP_Text statusText;

    [Header("Managers")]
    [SerializeField]
    private FirebaseGoogleAuthService googleAuthService;

    [SerializeField]
    private MainButton mainButton;

    [Header("Animation")]
    [SerializeField]
    private float fadeDuration = 0.3f;

    private bool isProcessing;
    private Tween fadeTween;

    private IEnumerator Start()
    {
        // 같은 오브젝트에 있는 MainButton을
        // 자동으로 찾을 수 있게 한다.
        if (mainButton == null)
        {
            mainButton =
                GetComponent<MainButton>();
        }

        // 같은 오브젝트에 있는 Google Auth Service도
        // 자동으로 찾는다.
        if (googleAuthService == null)
        {
            googleAuthService =
                GetComponent<FirebaseGoogleAuthService>();
        }

        // 처음에는 절대 화면 터치 시작 불가.
        if (mainButton != null)
        {
            mainButton.HideStartPrompt();
        }

        // GameRoot가 MainScene 진입 전에
        // 이미 준비되는 구조지만 안전하게 기다린다.
        while (GameRoot.Instance == null)
        {
            yield return null;
        }

        GameAccountManager account =
            GameRoot.Instance.Account;

        if (account == null)
        {
            Debug.LogError(
                "[MainLogin] GameAccountManager가 없습니다."
            );

            SetStatus(
                "계정 시스템을 불러오지 못했습니다."
            );

            yield break;
        }

        // 이전 실행에서 이미 Google 계정으로
        // 로그인된 상태가 복원되었다.
        if (account.IsGoogleAccount)
        {
            Debug.Log(
                $"[MainLogin] Google 로그인 세션 복원 | " +
                $"UID: {account.Data.uid}"
            );

            HideLoginUIImmediately();

            if (mainButton != null)
            {
                mainButton.ShowStartPrompt();
            }

            yield break;
        }

        // Guest 상태면 Google/Guest 선택창 표시.
        ShowLoginUI();

        Debug.Log(
            $"[MainLogin] 로그인 선택 대기 | " +
            $"Guest: {account.IsGuestAccount} | " +
            $"UID: {account.Data.uid}"
        );
    }

    public async void OnGoogleLoginButton()
    {
        if (isProcessing)
            return;

        if (googleAuthService == null)
        {
            Debug.LogError(
                "[MainLogin] FirebaseGoogleAuthService가 없습니다."
            );

            SetStatus(
                "Google 로그인 서비스를 찾지 못했습니다."
            );

            return;
        }

        isProcessing = true;

        SetButtonsInteractable(false);

        SetStatus(
            "Google 계정을 불러오는 중..."
        );

        bool success =
            await googleAuthService
                .SignInWithGoogleAsync();

        if (!success)
        {
            isProcessing = false;

            SetButtonsInteractable(true);

            if (googleAuthService
                .LastOperationWasCancelled)
            {
                SetStatus("");
            }
            else
            {
                string error =
                    googleAuthService.LastError;

                SetStatus(
                    string.IsNullOrWhiteSpace(error)
                        ? "Google 로그인에 실패했습니다."
                        : error
                );
            }

            return;
        }

        Debug.Log(
            "[MainLogin] Google 로그인 완료."
        );

        SetStatus("");

        FinishLogin();
    }

    public async void OnGuestContinueButton()
    {
        if (isProcessing)
            return;

        isProcessing = true;

        SetButtonsInteractable(false);

        GameAccountManager account =
            GameRoot.Instance?.Account;

        if (account == null)
        {
            Debug.LogError(
                "[MainLogin] GameAccountManager가 없습니다."
            );

            isProcessing = false;

            SetButtonsInteractable(true);

            SetStatus(
                "계정 시스템을 불러오지 못했습니다."
            );

            return;
        }

        // 원래 GameRoot에서 이미 Guest UID가
        // 만들어져 있어야 한다.
        //
        // 혹시 없다면 다시 한 번 확보.
        if (!account.HasCloudAccount)
        {
            SetStatus(
                "게스트 계정을 준비하는 중..."
            );

            bool success =
                await account
                    .EnsureSignedInAsync();

            if (!success)
            {
                Debug.LogError(
                    "[MainLogin] Guest 로그인 실패."
                );

                isProcessing = false;

                SetButtonsInteractable(true);

                SetStatus(
                    "게스트 계정을 준비하지 못했습니다."
                );

                return;
            }
        }

        Debug.Log(
            $"[MainLogin] Guest 진행 | " +
            $"UID: {account.Data.uid}"
        );

        SetStatus("");

        FinishLogin();
    }

    private void FinishLogin()
    {
        SetButtonsInteractable(false);

        fadeTween?.Kill();

        if (loginCanvasGroup == null)
        {
            HideLoginObjects();

            if (mainButton != null)
            {
                mainButton.ShowStartPrompt();
            }

            return;
        }

        fadeTween =
            loginCanvasGroup
                .DOFade(
                    0f,
                    Mathf.Max(
                        0.01f,
                        fadeDuration
                    )
                )
                .SetUpdate(true)
                .OnComplete(
                    () =>
                    {
                        loginCanvasGroup
                            .gameObject
                            .SetActive(false);

                        if (mainButton != null)
                        {
                            mainButton
                                .ShowStartPrompt();
                        }
                    }
                );
    }

    private void ShowLoginUI()
    {
        if (loginCanvasGroup != null)
        {
            loginCanvasGroup
                .gameObject
                .SetActive(true);

            loginCanvasGroup.alpha = 1f;
            loginCanvasGroup.interactable = true;
            loginCanvasGroup.blocksRaycasts = true;
        }
        else
        {
            if (googleLoginButton != null)
            {
                googleLoginButton
                    .gameObject
                    .SetActive(true);
            }

            if (guestContinueButton != null)
            {
                guestContinueButton
                    .gameObject
                    .SetActive(true);
            }
        }

        SetButtonsInteractable(true);

        SetStatus("");
    }

    private void HideLoginUIImmediately()
    {
        fadeTween?.Kill();

        if (loginCanvasGroup != null)
        {
            loginCanvasGroup.alpha = 0f;
            loginCanvasGroup.interactable = false;
            loginCanvasGroup.blocksRaycasts = false;

            loginCanvasGroup
                .gameObject
                .SetActive(false);

            return;
        }

        HideLoginObjects();
    }

    private void HideLoginObjects()
    {
        if (googleLoginButton != null)
        {
            googleLoginButton
                .gameObject
                .SetActive(false);
        }

        if (guestContinueButton != null)
        {
            guestContinueButton
                .gameObject
                .SetActive(false);
        }
    }

    private void SetButtonsInteractable(
        bool interactable)
    {
        if (googleLoginButton != null)
        {
            googleLoginButton.interactable =
                interactable;
        }

        if (guestContinueButton != null)
        {
            guestContinueButton.interactable =
                interactable;
        }

        if (loginCanvasGroup != null)
        {
            loginCanvasGroup.interactable =
                interactable;

            loginCanvasGroup.blocksRaycasts =
                interactable;
        }
    }

    private void SetStatus(
        string message)
    {
        if (statusText == null)
            return;

        statusText.text =
            message ?? "";
    }

    private void OnDestroy()
    {
        fadeTween?.Kill();
    }
}