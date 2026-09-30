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
            DebugPanelState();
        }

        if (panel3 != null)
        {
            panel3.SetActive(false);
        }
    }

    private void DebugPanelState()
    {
        RectTransform rt = settingPanel.GetComponent<RectTransform>();
        CanvasGroup cg = settingPanel.GetComponent<CanvasGroup>();
        Canvas canvas = settingPanel.GetComponentInParent<Canvas>();

        Debug.Log(
            $"[SettingPanel] 오브젝트: {settingPanel.name} (씬: {settingPanel.scene.name})\n" +
            $"activeInHierarchy: {settingPanel.activeInHierarchy}\n" +
            $"scale: {rt.localScale} / lossyScale: {rt.lossyScale}\n" +
            $"anchoredPos: {rt.anchoredPosition} / size: {rt.rect.size}\n" +
            $"CanvasGroup alpha: {(cg != null ? cg.alpha.ToString() : "없음")}\n" +
            $"layer: {LayerMask.LayerToName(settingPanel.layer)}\n" +
            $"Canvas: {canvas?.name} / renderMode: {canvas?.renderMode} / sortOrder: {canvas?.sortingOrder}\n" +
            $"자식 수: {settingPanel.transform.childCount}",
            settingPanel // 로그 클릭하면 해당 오브젝트 하이라이트됨
        );
    }

    public void CloseSetting()
    {
        // Bootstrap 없이 LobbyScene에서 바로 Play하면 GameRoot가 없을 수 있어서 null 체크
        if (GameRoot.Instance != null && GameRoot.Instance.Settings != null)
        {
            GameRoot.Instance.Settings.SaveNow();
        }

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
