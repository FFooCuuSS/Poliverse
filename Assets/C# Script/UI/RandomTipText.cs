using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RandomTipText : MonoBehaviour
{
    [SerializeField] private string[] messages;

    [SerializeField] private TMP_Text displayText;

    void Start()
    {
        ShowRandomMessage();
    }

    void ShowRandomMessage()
    {
        if (messages.Length == 0)
        {
            return;
        }

        int index = Random.Range(0, messages.Length);
        string randomMessage = messages[index];

        if (displayText != null)
        {
            displayText.text = randomMessage;
        }   
    }
}
