using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingPopup : MonoBehaviour
{
    [SerializeField] private GameObject setting;

    public void closePopup()
    {
        setting.SetActive(false);
    }
}
