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

    public bool IsGuestAccount =>
        HasCloudAccount &&
        Data.isAnonymous;

    public bool IsGoogleAccount =>
        HasCloudAccount &&
        !Data.isAnonymous &&
        Data.providerId == "google.com";

    public FirebaseUser CurrentFirebaseUser =>
        auth?.CurrentUser;

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

            if (status != DependencyStatus.Available)
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

    /// <summary>
    /// Firebase 초기화 후 반드시 호출.
    ///
    /// 기존 로그인 세션이 있으면 그대로 사용하고,
    /// 없으면 Firebase Anonymous 계정을 생성한다.
    /// </summary>
    public async Task<bool> EnsureSignedInAsync()
    {
        if (!IsFirebaseReady ||
            auth == null)
        {
            Debug.LogWarning(
                "[Account] Firebase가 준비되지 않아 " +
                "로컬 계정으로 계속합니다."
            );

            return false;
        }

        FirebaseUser existingUser =
            auth.CurrentUser;

        if (existingUser != null)
        {
            ApplyCloudIdentity(
                existingUser
            );

            Debug.Log(
                $"[Account] 기존 Firebase 계정 사용 | " +
                $"UID: {existingUser.UserId} | " +
                $"Anonymous: {existingUser.IsAnonymous}"
            );

            return true;
        }

        return await SignInAnonymouslyAsync();
    }

    /// <summary>
    /// Firebase Guest 계정을 생성한다.
    /// 생성된 사용자 역시 정상적인 Firebase UID를 가진다.
    /// </summary>
    public async Task<bool> SignInAnonymouslyAsync()
    {
        if (!IsFirebaseReady ||
            auth == null)
        {
            Debug.LogError(
                "[Account] Firebase가 준비되지 않았습니다."
            );

            return false;
        }

        // 이미 로그인되어 있다면 새 익명 계정을 만들지 않는다.
        if (auth.CurrentUser != null)
        {
            ApplyCloudIdentity(
                auth.CurrentUser
            );

            return true;
        }

        try
        {
            AuthResult result =
                await auth
                    .SignInAnonymouslyAsync();

            FirebaseUser user =
                result?.User;

            if (user == null)
            {
                Debug.LogError(
                    "[Account] 익명 로그인 결과가 null입니다."
                );

                return false;
            }

            ApplyCloudIdentity(
                user
            );

            Debug.Log(
                $"[Account] Guest 로그인 성공 | " +
                $"UID: {user.UserId}"
            );

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[Account] Guest 로그인 실패\n" +
                $"{exception}"
            );

            return false;
        }
    }

    /// <summary>
    /// Google 등 이미 존재하는 Firebase 계정으로
    /// 직접 로그인할 때 사용한다.
    ///
    /// Guest → Google 업그레이드에는
    /// LinkCurrentAnonymousWithCredentialAsync를 사용한다.
    /// </summary>
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

            ApplyCloudIdentity(
                user
            );

            Debug.Log(
                $"[Account] Firebase 계정 로그인 성공 | " +
                $"UID: {user.UserId}"
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

    /// <summary>
    /// 현재 Firebase Anonymous 계정에
    /// Google Credential 등을 연결한다.
    ///
    /// 성공하면 Guest UID가 그대로 유지된다.
    /// 따라서 Firestore 세이브 경로도 변하지 않는다.
    /// </summary>
    public async Task<bool>
        LinkCurrentAnonymousWithCredentialAsync(
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
                "[Account] 연결 Credential이 null입니다."
            );

            return false;
        }

        FirebaseUser currentUser =
            auth.CurrentUser;

        if (currentUser == null)
        {
            Debug.LogError(
                "[Account] 연결할 Firebase 사용자가 없습니다."
            );

            return false;
        }

        if (!currentUser.IsAnonymous)
        {
            Debug.LogWarning(
                "[Account] 현재 사용자는 Guest 계정이 아닙니다."
            );

            return false;
        }

        string previousUid =
            currentUser.UserId;

        try
        {
            AuthResult result =
                await currentUser
                    .LinkWithCredentialAsync(
                        credential
                    );

            FirebaseUser linkedUser =
                result?.User;

            if (linkedUser == null)
            {
                Debug.LogError(
                    "[Account] 계정 연결 결과가 null입니다."
                );

                return false;
            }

            ApplyCloudIdentity(
                linkedUser
            );

            Debug.Log(
                $"[Account] Guest 계정 업그레이드 성공\n" +
                $"- Before UID: {previousUid}\n" +
                $"- After UID: {linkedUser.UserId}\n" +
                $"- Provider: {GetProviderId(linkedUser)}"
            );

            return true;
        }
        catch (Exception exception)
        {
            // 여기에는
            // "이 Google Credential이 이미 다른 Firebase 계정에 연결됨"
            // 같은 경우도 들어온다.
            //
            // 그 경우 로그인 UI 단계에서
            // Guest Save + 기존 Google Save 병합 흐름으로 처리한다.

            Debug.LogError(
                $"[Account] Guest 계정 연결 실패\n" +
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

        ApplyCloudIdentity(
            user
        );
    }

    public void ApplyCloudIdentity(
        FirebaseUser user)
    {
        EnsureInitialized();

        if (user == null ||
            string.IsNullOrWhiteSpace(
                user.UserId))
        {
            Debug.LogError(
                "[Account] 유효하지 않은 Firebase 사용자입니다."
            );

            return;
        }

        string providerId =
            GetProviderId(
                user
            );

        bool changed =
            Data.uid != user.UserId ||
            !Data.isAuthenticated ||
            Data.isAnonymous !=
                user.IsAnonymous ||
            Data.providerId !=
                providerId ||
            Data.email !=
                (user.Email ?? "") ||
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
            $"- Anonymous: {Data.isAnonymous}\n" +
            $"- Provider: {Data.providerId}\n" +
            $"- Email: {Data.email}"
        );

        if (changed)
            OnAccountChanged?.Invoke();
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
            !string.IsNullOrWhiteSpace(
                Data.uid
            );

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
                "[Account] 현재 Firebase 계정이 없습니다."
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