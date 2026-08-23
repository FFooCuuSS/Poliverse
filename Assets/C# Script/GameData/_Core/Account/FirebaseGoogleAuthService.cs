using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using UnityEngine;

public class FirebaseGoogleAuthService : MonoBehaviour
{
    // google-services.json의 client_type = 3
    // Web OAuth Client ID.
    //
    // Android Client ID가 아니다.
    private const string WebClientId =
        "651529722196-2pn2mi0u1faudg9qef7tnmpaf1q2lqc8.apps.googleusercontent.com";

    private TaskCompletionSource<string>
        googleTokenCompletionSource;

    public bool IsBusy { get; private set; }

    public string LastError { get; private set; }

    public bool LastOperationWasCancelled
    {
        get;
        private set;
    }

    public async Task<bool> SignInWithGoogleAsync()
    {
        if (IsBusy)
            return false;

        LastError = "";
        LastOperationWasCancelled = false;

        GameAccountManager account =
            GameRoot.Instance?.Account;

        if (account == null)
        {
            LastError =
                "계정 시스템을 찾지 못했습니다.";

            Debug.LogError(
                "[GoogleAuth] GameAccountManager가 없습니다."
            );

            return false;
        }

        if (!account.IsFirebaseReady)
        {
            LastError =
                "Firebase가 준비되지 않았습니다.";

            Debug.LogError(
                "[GoogleAuth] Firebase가 준비되지 않았습니다."
            );

            return false;
        }

        GameSaveManager saveManager =
            FindObjectOfType<GameSaveManager>();

        IsBusy = true;

        try
        {
            Debug.Log(
                "[GoogleAuth] Google Credential 요청 시작."
            );

            string idToken =
                await RequestGoogleIdTokenAsync();

            if (string.IsNullOrWhiteSpace(
                    idToken))
            {
                LastError =
                    "Google 인증 토큰을 받지 못했습니다.";

                return false;
            }

            Debug.Log(
                "[GoogleAuth] Google ID Token 수신 완료."
            );

            Credential credential =
                GoogleAuthProvider
                    .GetCredential(
                        idToken,
                        null
                    );

            if (credential == null)
            {
                LastError =
                    "Firebase Credential 생성에 실패했습니다.";

                return false;
            }

            // 이미 Google 계정이라면
            // 굳이 Guest Link를 시도하지 않는다.
            if (account.IsGoogleAccount)
            {
                return await account
                    .SignInWithCredentialAsync(
                        credential
                    );
            }

            if (!account.IsGuestAccount)
            {
                bool directSignIn =
                    await account
                        .SignInWithCredentialAsync(
                            credential
                        );

                if (!directSignIn)
                {
                    LastError =
                        "Google Firebase 로그인에 실패했습니다.";
                }

                return directSignIn;
            }

            // ------------------------------------------------
            // 여기부터 Guest → Google
            // ------------------------------------------------

            GameSaveData guestSnapshot =
                CloneSaveData(
                    saveManager?.Data
                );

            return await LinkGuestOrRecoverExistingGoogleAsync(
                account,
                saveManager,
                guestSnapshot,
                credential
            );
        }
        catch (Exception exception)
        {
            string message =
                exception.Message ?? "";

            if (message.StartsWith(
                    "CANCELLED|",
                    StringComparison.Ordinal))
            {
                LastOperationWasCancelled =
                    true;

                LastError = "";

                Debug.Log(
                    "[GoogleAuth] 사용자가 Google 로그인을 취소했습니다."
                );

                return false;
            }

            LastError =
                "Google 로그인 중 오류가 발생했습니다.";

            Debug.LogError(
                $"[GoogleAuth] 로그인 예외\n" +
                $"{exception}"
            );

            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<bool>
        LinkGuestOrRecoverExistingGoogleAsync(
            GameAccountManager account,
            GameSaveManager saveManager,
            GameSaveData guestSnapshot,
            Credential credential)
    {
        FirebaseUser guestUser =
            account.CurrentFirebaseUser;

        if (guestUser == null)
        {
            LastError =
                "현재 Guest Firebase 계정을 찾지 못했습니다.";

            return false;
        }

        if (!guestUser.IsAnonymous)
        {
            LastError =
                "현재 Firebase 계정이 Guest가 아닙니다.";

            return false;
        }

        string guestUid =
            guestUser.UserId;

        try
        {
            Debug.Log(
                $"[GoogleAuth] Guest → Google Link 시도 | " +
                $"UID: {guestUid}"
            );

            AuthResult linkResult =
                await guestUser
                    .LinkWithCredentialAsync(
                        credential
                    );

            FirebaseUser linkedUser =
                linkResult?.User;

            if (linkedUser == null)
            {
                LastError =
                    "Google 계정 연결 결과가 비어 있습니다.";

                return false;
            }

            // Link라면 UID가 절대 바뀌면 안 된다.
            if (linkedUser.UserId != guestUid)
            {
                LastError =
                    "Guest 연결 후 UID가 변경되었습니다.";

                Debug.LogError(
                    $"[GoogleAuth] UID 변경 감지\n" +
                    $"Before: {guestUid}\n" +
                    $"After: {linkedUser.UserId}"
                );

                return false;
            }

            account.ApplyCloudIdentity(
                linkedUser
            );

            Debug.Log(
                $"[GoogleAuth] Guest → Google 연결 성공\n" +
                $"UID 유지: {linkedUser.UserId}\n" +
                $"Email: {linkedUser.Email}"
            );

            return true;
        }
        catch (FirebaseAccountLinkException exception)
        {
            AuthError error =
                (AuthError)exception.ErrorCode;

            Debug.LogWarning(
                $"[GoogleAuth] Link 실패 | " +
                $"AuthError: {error}\n" +
                $"{exception.Message}"
            );

            // 이미 다른 Firebase User가
            // 해당 Google Credential을 사용 중.
            //
            // 대표적인 재설치/기존 계정 로그인 케이스.
            if (error ==
                    AuthError.CredentialAlreadyInUse ||
                error ==
                    AuthError.EmailAlreadyInUse ||
                error ==
                    AuthError.AccountExistsWithDifferentCredentials)
            {
                return await
                    RecoverExistingGoogleAccountAsync(
                        account,
                        saveManager,
                        guestSnapshot,
                        credential
                    );
            }

            LastError =
                $"Google 계정 연결 실패: {error}";

            return false;
        }
        catch (FirebaseException exception)
        {
            AuthError error =
                (AuthError)exception.ErrorCode;

            Debug.LogError(
                $"[GoogleAuth] Firebase Link 오류 | " +
                $"{error}\n" +
                $"{exception}"
            );

            LastError =
                $"Google 계정 연결 실패: {error}";

            return false;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[GoogleAuth] Guest Link 예외\n" +
                $"{exception}"
            );

            LastError =
                "Google 계정 연결 중 오류가 발생했습니다.";

            return false;
        }
    }

    private async Task<bool>
        RecoverExistingGoogleAccountAsync(
            GameAccountManager account,
            GameSaveManager saveManager,
            GameSaveData guestSnapshot,
            Credential credential)
    {
        if (saveManager == null ||
            saveManager.Data == null)
        {
            LastError =
                "세이브 시스템을 찾지 못해 기존 Google 계정으로 안전하게 전환할 수 없습니다.";

            Debug.LogError(
                "[GoogleAuth] 기존 Google 계정 복구 전에 " +
                "GameSaveManager가 필요합니다."
            );

            return false;
        }

        if (guestSnapshot == null)
        {
            LastError =
                "현재 Guest 세이브를 백업하지 못했습니다.";

            return false;
        }

        Debug.Log(
            "[GoogleAuth] 기존 Google Firebase 계정 발견. " +
            "Guest 데이터를 보관한 뒤 기존 계정 로그인 시작."
        );

        bool signInSuccess =
            await account
                .SignInWithCredentialAsync(
                    credential
                );

        if (!signInSuccess ||
            !account.IsGoogleAccount)
        {
            LastError =
                "기존 Google 계정 로그인에 실패했습니다.";

            return false;
        }

        string googleUid =
            account.Data.uid;

        Debug.Log(
            $"[GoogleAuth] 기존 Google 계정 로그인 성공 | " +
            $"UID: {googleUid}"
        );

        // GameSaveManager.HandleAccountChanged()는 async void라
        // Google UID의 Firestore 데이터 로드가 끝날 때까지 기다린다.
        bool saveAccountReady =
            await WaitUntilSaveOwnerIsAsync(
                saveManager,
                googleUid,
                12f
            );

        if (!saveAccountReady)
        {
            LastError =
                "Google Cloud 세이브 로드를 기다리다 시간이 초과되었습니다.";

            Debug.LogError(
                $"[GoogleAuth] SaveManager 계정 전환 Timeout | " +
                $"UID: {googleUid}"
            );

            return false;
        }

        GameSaveData googleSave =
            CloneSaveData(
                saveManager.Data
            );

        GameSaveData merged =
            GameSaveMerger.Merge(
                guestSnapshot,
                googleSave
            );

        if (merged == null)
        {
            LastError =
                "Guest와 Google 세이브 병합에 실패했습니다.";

            return false;
        }

        merged.ownerUid =
            googleUid;

        // GameSaveManager.Data는 private setter이므로
        // 객체 자체는 유지하면서 내용만 덮어쓴다.
        string mergedJson =
            JsonUtility.ToJson(
                merged
            );

        JsonUtility.FromJsonOverwrite(
            mergedJson,
            saveManager.Data
        );

        saveManager.Data.ownerUid =
            googleUid;

        Debug.Log(
            $"[GoogleAuth] Guest + Google Save 병합 완료 | " +
            $"Owner UID: {googleUid}"
        );

        saveManager.RequestSave();

        bool saveFinished =
            await WaitUntilSaveFinishedAsync(
                saveManager,
                12f
            );

        if (!saveFinished)
        {
            LastError =
                "병합 세이브 저장이 시간 안에 완료되지 않았습니다.";

            return false;
        }

        Debug.Log(
            $"[GoogleAuth] 기존 Google 계정 복구 및 병합 완료 | " +
            $"UID: {googleUid}"
        );

        return true;
    }

    private async Task<bool>
        WaitUntilSaveOwnerIsAsync(
            GameSaveManager saveManager,
            string targetUid,
            float timeoutSeconds)
    {
        DateTime endTime =
            DateTime.UtcNow.AddSeconds(
                Mathf.Max(
                    1f,
                    timeoutSeconds
                )
            );

        while (DateTime.UtcNow <
               endTime)
        {
            if (saveManager != null &&
                saveManager.Data != null &&
                saveManager.Data.ownerUid ==
                    targetUid)
            {
                return true;
            }

            await Task.Delay(100);
        }

        return false;
    }

    private async Task<bool>
        WaitUntilSaveFinishedAsync(
            GameSaveManager saveManager,
            float timeoutSeconds)
    {
        DateTime endTime =
            DateTime.UtcNow.AddSeconds(
                Mathf.Max(
                    1f,
                    timeoutSeconds
                )
            );

        // RequestSave() 직후 저장 코루틴이
        // 시작된 상태이므로 완료까지 기다린다.
        while (DateTime.UtcNow <
               endTime)
        {
            if (saveManager == null)
                return false;

            if (!saveManager.IsSaving)
                return true;

            await Task.Delay(100);
        }

        return false;
    }

    private GameSaveData CloneSaveData(
        GameSaveData source)
    {
        if (source == null)
            return null;

        string json =
            JsonUtility.ToJson(
                source
            );

        return JsonUtility
            .FromJson<GameSaveData>(
                json
            );
    }

    private Task<string>
        RequestGoogleIdTokenAsync()
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        if (googleTokenCompletionSource != null &&
            !googleTokenCompletionSource
                .Task
                .IsCompleted)
        {
            return googleTokenCompletionSource.Task;
        }

        googleTokenCompletionSource =
            new TaskCompletionSource<string>(
                TaskCreationOptions
                    .RunContinuationsAsynchronously
            );

        try
        {
            using AndroidJavaClass
                unityPlayer =
                    new AndroidJavaClass(
                        "com.unity3d.player.UnityPlayer"
                    );

            using AndroidJavaObject
                activity =
                    unityPlayer
                        .GetStatic<AndroidJavaObject>(
                            "currentActivity"
                        );

            using AndroidJavaClass
                bridge =
                    new AndroidJavaClass(
                        "com.hong.copspace.auth.GoogleCredentialBridge"
                    );

            bridge.CallStatic(
                "signIn",
                activity,
                WebClientId,
                gameObject.name,
                nameof(
                    OnGoogleCredentialSuccess
                ),
                nameof(
                    OnGoogleCredentialError
                )
            );
        }
        catch (Exception exception)
        {
            googleTokenCompletionSource
                .TrySetException(
                    exception
                );
        }

        return googleTokenCompletionSource.Task;

#else

        return Task.FromException<string>(
            new InvalidOperationException(
                "Google Credential Manager는 " +
                "Unity Editor에서 실행할 수 없습니다. " +
                "Android 실기기에서 테스트하세요."
            )
        );

#endif
    }

    // Java의 UnitySendMessage가 호출한다.
    public void OnGoogleCredentialSuccess(
        string idToken)
    {
        if (googleTokenCompletionSource ==
            null)
        {
            return;
        }

        googleTokenCompletionSource
            .TrySetResult(
                idToken
            );
    }

    // Java의 UnitySendMessage가 호출한다.
    public void OnGoogleCredentialError(
        string errorMessage)
    {
        if (googleTokenCompletionSource ==
            null)
        {
            return;
        }

        googleTokenCompletionSource
            .TrySetException(
                new InvalidOperationException(
                    errorMessage ??
                    "UNKNOWN_GOOGLE_ERROR"
                )
            );
    }
}