using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingItemsButton : MonoBehaviour
{
    // 버튼 하나 + 그 버튼을 눌렀을 때 보여줄 패널 하나를 한 쌍으로 묶음
    [System.Serializable]
    public class SettingTab
    {
        public Button button;
        public GameObject contents;
    }

    [Header("탭 목록 (0번이 기본 탭)")]
    public List<SettingTab> tabs = new List<SettingTab>();
    public int defaultTabIndex = 0;

    [Header("버튼 색")]
    public Color selectedColor = new Color(0.207f, 0.316f, 0.384f);
    public Color defaultColor = Color.white;

    private void Awake()
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            int index = i; // 람다 안에서 i를 그대로 쓰면 전부 마지막 값이 되므로 복사
            if (tabs[i].button != null)
                tabs[i].button.onClick.AddListener(() => SelectTab(index));
        }
    }

    // 설정창이 켜질 때마다 기본 탭(Sound)으로 시작
    private void OnEnable()
    {
        SelectTab(defaultTabIndex);
    }

    public void SelectTab(int index)
    {
        if (index < 0 || index >= tabs.Count)
            return;

        for (int i = 0; i < tabs.Count; i++)
        {
            bool isSelected = (i == index);

            // 패널: 선택된 것만 켜기
            if (tabs[i].contents != null)
                tabs[i].contents.SetActive(isSelected);

            // 버튼 색: 선택된 것만 강조
            if (tabs[i].button != null)
            {
                Color targetColor = isSelected ? selectedColor : defaultColor;
                ColorBlock cb = tabs[i].button.colors;
                cb.normalColor = targetColor;
                cb.highlightedColor = targetColor;
                cb.pressedColor = targetColor;
                cb.selectedColor = targetColor;
                tabs[i].button.colors = cb;
            }
        }
    }
}