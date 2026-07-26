using System;
using UnityEngine;

public class GameAccountManager : MonoBehaviour
{
    private const string LocalInstallIdKey =
        "GAME_LOCAL_INSTALL_ID";

    public GameAccountData Data { get; private set; }

    public bool IsInitialized => Data != null;

    public bool HasCloudAccount =>
        Data != null &&
        Data.isAuthenticated &&
        !string.IsNullOrWhiteSpace(Data.uid);

    public void InitializeLocalOnly()
    {
        string localInstallId =
            PlayerPrefs.GetString(LocalInstallIdKey, "");

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
            providerId = "local"
        };

        // FIREBASE-LATER:
        // 1. Firebase 초기화
        // 2. 기존 로그인 세션 확인
        // 3. 세션이 없으면 익명 로그인
        // 4. Firebase UID를 ApplyCloudIdentity에 전달
    }

    /// <summary>
    /// 나중에 FirebaseAuthService가 인증에 성공했을 때 호출할 자리.
    /// 현재 코드에서는 호출하지 않는다.
    /// </summary>
    public void ApplyCloudIdentity(
        string uid,
        bool isAnonymous,
        string providerId)
    {
        EnsureInitialized();

        if (string.IsNullOrWhiteSpace(uid))
        {
            Debug.LogError(
                "[Account] 비어 있는 UID는 적용할 수 없습니다."
            );

            return;
        }

        Data.uid = uid;
        Data.isAuthenticated = true;
        Data.isAnonymous = isAnonymous;
        Data.providerId =
            string.IsNullOrWhiteSpace(providerId)
                ? "unknown"
                : providerId;
    }

    public void ClearCloudIdentity()
    {
        EnsureInitialized();

        Data.uid = "";
        Data.isAuthenticated = false;
        Data.isAnonymous = true;
        Data.providerId = "local";
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
                "[Account] 현재는 로컬 전용 상태입니다. " +
                "Firebase 계정 연동은 아직 구현되지 않았습니다."
            );
        }
    }

    private void EnsureInitialized()
    {
        if (Data == null)
            InitializeLocalOnly();
    }
}