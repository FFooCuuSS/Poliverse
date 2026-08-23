using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainButtonBasicSelect : MonoBehaviour
{
    public GameObject[] defaultImages;
    public GameObject[] hoverImages;

    public void ShowHover()
    {
        foreach (var go in defaultImages)
            go.SetActive(false);

        foreach (var go in hoverImages)
            go.SetActive(true);
    }

    public void ShowDefault()
    {
        foreach (var go in defaultImages)
            go.SetActive(true);

        foreach (var go in hoverImages)
            go.SetActive(false);
    }
}
