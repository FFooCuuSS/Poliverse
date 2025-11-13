using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompletedMacaroon : MonoBehaviour
{
    public Sprite[] completedMacaroonImage;
    private Image targetImage;

    void Start()
    {
        targetImage = GetComponent<Image>();

        if (completedMacaroonImage.Length > 0)
        {
            int randomIndex = Random.Range(0, completedMacaroonImage.Length);
            targetImage.sprite = completedMacaroonImage[randomIndex];
        }
    }
}
