using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OptionClickHandler : MonoBehaviour
{
    public int optionIndex;
    public MontageManager manager;

    void OnMouseDown()
    {
        Debug.Log("Å¬¸¯µÊ");
        manager.CheckAnswer(optionIndex);
    }
}
