using System;

[Serializable]
public class GameAccountData
{
    public string localInstallId = "";

    public string uid = "";

    public bool isAuthenticated = false;
    public bool isAnonymous = true;

    public string providerId = "local";

    public string email = "";
    public string displayName = "";
}