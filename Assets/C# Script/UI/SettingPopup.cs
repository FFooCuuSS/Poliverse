using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingPopup : MonoBehaviour
{
    [Header("연결할 패널")]
    public GameObject settingPanel;
    public GameObject panel3;

    void Start()
    {
        if (settingPanel != null)
        {
            settingPanel.SetActive(false);
        }  
    }

    public void OpenSetting()
    {
        if (settingPanel != null)
        {
            settingPanel.SetActive(true);
        }
            
        if (panel3 != null)
        {
            panel3.SetActive(false);
        } 
    }

    public void CloseSetting()
    {
        if (settingPanel != null)
        {
            settingPanel.SetActive(false);
        }
            
        if (panel3 != null)
        {
            panel3.SetActive(true);
        }
    }
}
