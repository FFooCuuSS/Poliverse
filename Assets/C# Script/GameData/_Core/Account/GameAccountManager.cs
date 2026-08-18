using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using UnityEngine;

public class GameAccountManager : MonoBehaviour
{
    private const string LocalInstallIdKey =
        "GAME_LOCAL_INSTALL_ID";

    private FirebaseAuth auth;

    public GameAccountData Data { get; private set; }

    public bool IsFirebaseReady { get; private set; }

    public bool IsInitialized =>
        Data != null;

    public bool HasCloudAccount =>
        Data != null &&
        Data.isAuthenticated &&
        !string.IsNullOrWhiteSpace(Data.uid);

    public event Action OnAccountChanged;

    public void InitializeLocalOnly()
    {
        if (Data != null)
            return;

        string localInstallId =
            PlayerPrefs.GetString(
                LocalInstallIdKey,
                ""
            );

        if (string.IsNullOrWhiteSpace(localInstallId))
        {
            localInstallId =
                Guid.NewGuid().ToString("N");

            PlayerPrefs.SetString(
                LocalInstallIdKey,
                localInstallId
            );

            PlayerPrefs.Save();
        }

        Data = new GameAccountData
        {
            localInstallId = localInstallId,

            uid = "",

            isAuthenticated = false,
            isAnonymous = true,

            providerId = "local",

            email = "",
            displayName = ""
        };
    }

    public async Task<bool> InitializeFirebaseAsync()
    {
        EnsureInitialized();

        try
        {
            DependencyStatus status =
                await FirebaseApp
                    .CheckAndFixDependenciesAsync();

            if (status !=
                DependencyStatus.Available)
            {
                IsFirebaseReady = false;

                Debug.LogError(
                    $"[Account] Firebase 초기화 실패: " +
                    $"{status}"
                );

                return false;
            }

            auth = FirebaseAuth.DefaultInstance;

            auth.StateChanged -=
                HandleAuthStateChanged;

            auth.StateChanged +=
                HandleAuthStateChanged;

            IsFirebaseReady = true;

            Debug.Log(
                "[Account] Firebase 초기화 성공."
            );

            ApplyCurrentFirebaseUser();

            return true;
        }
        catch (Exception exception)
        {
            IsFirebaseReady = false;

            Debug.LogError(
                $"[Account] Firebase 초기화 중 예외 발생\n" +
                $"{exception}"
            );

            return false;
        }
    }

    private void HandleAuthStateChanged(
        object sender,
        EventArgs eventArgs)
    {
        ApplyCurrentFirebaseUser();
    }

    private void ApplyCurrentFirebaseUser()
    {
        if (auth == null)
            return;

        FirebaseUser user =
            auth.CurrentUser;

        if (user == null)
        {
            bool changed =
                Data != null &&
                Data.isAuthenticated;

            ClearCloudIdentityInternal(
                notify: changed
            );

            Debug.Log(
                "[Account] Firebase 로그인 세션 없음."
            );

            return;
        }

        ApplyCloudIdentity(user);
    }

    public void ApplyCloudIdentity(
        FirebaseUser user)
    {
        EnsureInitialized();

        if (user == null ||
            string.IsNullOrWhiteSpace(user.UserId))
        {
            Debug.LogError(
                "[Account] 유효하지 않은 Firebase 사용자입니다."
            );

            return;
        }

        string providerId =
            GetProviderId(user);

        bool changed =
            Data.uid != user.UserId ||
            !Data.isAuthenticated ||
            Data.isAnonymous != user.IsAnonymous ||
            Data.providerId != providerId ||
            Data.email != (user.Email ?? "") ||
            Data.displayName !=
                (user.DisplayName ?? "");

        Data.uid =
            user.UserId;

        Data.isAuthenticated =
            true;

        Data.isAnonymous =
            user.IsAnonymous;

        Data.providerId =
            providerId;

        Data.email =
            user.Email ?? "";

        Data.displayName =
            user.DisplayName ?? "";

        Debug.Log(
            $"[Account] Firebase 사용자 적용\n" +
            $"- UID: {Data.uid}\n" +
            $"- Provider: {Data.providerId}\n" +
            $"- Email: {Data.email}"
        );

        if (changed)
            OnAccountChanged?.Invoke();
    }

    public async Task<bool> SignInWithCredentialAsync(
        Credential credential)
    {
        if (!IsFirebaseReady ||
            auth == null)
        {
            Debug.LogError(
                "[Account] Firebase가 준비되지 않았습니다."
            );

            return false;
        }

        if (credential == null)
        {
            Debug.LogError(
                "[Account] 로그인 Credential이 null입니다."
            );

            return false;
        }

        try
        {
            FirebaseUser user =
                await auth
                    .SignInWithCredentialAsync(
                        credential
                    );

            if (user == null)
            {
                Debug.LogError(
                    "[Account] Firebase 로그인 결과가 null입니다."
                );

                return false;
            }

            ApplyCloudIdentity(user);

            Debug.Log(
                "[Account] Firebase 로그인 성공."
            );

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[Account] Firebase 로그인 실패\n" +
                $"{exception}"
            );

            return false;
        }
    }

    public void SignOut()
    {
        if (auth != null)
            auth.SignOut();

        ClearCloudIdentityInternal(
            notify: true
        );

        Debug.Log(
            "[Account] Firebase 로그아웃 완료."
        );
    }

    public void ClearCloudIdentity()
    {
        ClearCloudIdentityInternal(
            notify: true
        );
    }

    private void ClearCloudIdentityInternal(
        bool notify)
    {
        EnsureInitialized();

        bool changed =
            Data.isAuthenticated ||
            !string.IsNullOrWhiteSpace(Data.uid);

        Data.uid = "";

        Data.isAuthenticated = false;
        Data.isAnonymous = true;

        Data.providerId = "local";

        Data.email = "";
        Data.displayName = "";

        if (notify && changed)
            OnAccountChanged?.Invoke();
    }

    private string GetProviderId(
        FirebaseUser user)
    {
        if (user == null)
            return "unknown";

        foreach (IUserInfo provider
                 in user.ProviderData)
        {
            if (!string.IsNullOrWhiteSpace(
                    provider.ProviderId))
            {
                return provider.ProviderId;
            }
        }

        return user.IsAnonymous
            ? "anonymous"
            : "firebase";
    }

    public string GetCurrentOwnerKey()
    {
        EnsureInitialized();

        if (HasCloudAccount)
            return $"firebase:{Data.uid}";

        return $"local:{Data.localInstallId}";
    }

    public void WarnIfCloudUnavailable()
    {
        if (!HasCloudAccount)
        {
            Debug.LogWarning(
                "[Account] 현재는 로컬 전용 상태입니다."
            );
        }
    }

    private void EnsureInitialized()
    {
        if (Data == null)
            InitializeLocalOnly();
    }

    private void OnDestroy()
    {
        if (auth != null)
        {
            auth.StateChanged -=
                HandleAuthStateChanged;
        }
    }
}