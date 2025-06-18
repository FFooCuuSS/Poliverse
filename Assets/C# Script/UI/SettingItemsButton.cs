using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingItemsButton : MonoBehaviour
{
    public List<Button> settingButtons;

    public Color selectedColor = new Color(0.207f, 0.316f, 0.384f);
    public Color defaultColor = Color.white;

    private void Start()
    {
        foreach (Button btn in settingButtons)
        {
            btn.onClick.AddListener(() => OnSettingButtonClick(btn));
        }
    }

    void OnSettingButtonClick(Button clickedButton)
    {
        foreach (Button btn in settingButtons)
        {
            ColorBlock cb = btn.colors;

            bool isSelected = (btn == clickedButton);
            Color targetColor = isSelected ? selectedColor : defaultColor;

            cb.normalColor = targetColor;
            cb.highlightedColor = targetColor;
            cb.pressedColor = targetColor;
            cb.selectedColor = targetColor;

            btn.colors = cb;
        }
    }
}
