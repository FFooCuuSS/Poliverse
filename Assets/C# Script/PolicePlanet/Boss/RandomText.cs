using System.Collections;
using UnityEngine;
using TMPro;

public class RandomText : MonoBehaviour
{
    public TextMeshProUGUI instructionText;

    public string[] policeLines = {
        "Freeze!",
        "You're under arrest.",
        "Stop right there!",
        "We got a runner!",
        "Code 9 in progress!"
    };

    public string[] sinnerLines = {
        "I didn't do it!",
        "Let me go!",
        "You got the wrong guy!",
        "It wasn't me!",
        "I'm innocent!"
    };

    private Coroutine currentCoroutine;

    void Start()
    {
        instructionText.gameObject.SetActive(false);
    }

    public void ShowLine(bool isPolice)
    {
        string[] source = isPolice ? policeLines : sinnerLines;
        string randomLine = source[Random.Range(0, source.Length)];

        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        instructionText.text = randomLine;
        instructionText.gameObject.SetActive(true);
    }

    public void HideText()
    {
        instructionText.gameObject.SetActive(false);
    }
}
