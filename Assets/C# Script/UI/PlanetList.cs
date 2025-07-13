//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

//[System.Serializable]
//public class PlanetIconTextSet
//{
//    public Sprite icon;
//    public string text;
//}

//public class PlanetList : MonoBehaviour
//{
//    [SerializeField] private PlanetIconTextSet[] sets;

//    [SerializeField] private Image planeticonImage;
//    [SerializeField] private TMP_Text planettextUI;

//    private int currentIndex = 0;

//    void Start()
//    {
//        UpdateUI();
//    }

//    public void OnRightButton()
//    {
//        currentIndex++;
//        if (currentIndex >= sets.Length)
//            currentIndex = 0;

//        UpdateUI();
//    }

//    public void OnLeftButton()
//    {
//        currentIndex--;
//        if (currentIndex < 0)
//            currentIndex = sets.Length - 1;

//        UpdateUI();
//    }

//    private void UpdateUI()
//    {
//        planeticonImage.sprite = sets[currentIndex].icon;
//        planettextUI.text = sets[currentIndex].text;
//    }
//}
