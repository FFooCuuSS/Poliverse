using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Score : MonoBehaviour
{
    public TextMeshProUGUI myText;

    public int nScore;
    public int maxScore = 5;

    void Start()
    {
        UpdateText();
    }

    void Update()
    {
        UpdateText();
    }

    void UpdateText()
    {
        myText.text = $"{nScore} / {maxScore}";
    }

    public void SetMaxScore(int value)
    {
        maxScore = value;
    }
}