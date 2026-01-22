using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;


public class Score : MonoBehaviour
{
    public TextMeshProUGUI myText;

    public int nScore;

    void Start()
    {
        myText.text = $"{nScore} /5";
    }

    // Update is called once per frame
    void Update()
    {
        if (nScore > 15) return;
        myText.text = $"{nScore} /5";
    }
}
