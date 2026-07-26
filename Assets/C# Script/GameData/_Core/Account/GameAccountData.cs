using System;

[Serializable]
public class GameAccountData
{
    // 이 게임 설치본을 구분하기 위한 로컬 ID.
    // Firebase UID가 아니며 서버 인증에 사용하면 안 된다.
    public string localInstallId = "";

    // FIREBASE-LATER:
    // Firebase Authentication 로그인 후 UID 저장.
    public string uid = "";

    public bool isAuthenticated = false;
    public bool isAnonymous = true;

    // 예: firebase-anonymous, google, apple, email
    public string providerId = "local";
}