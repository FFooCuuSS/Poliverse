using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Macaron : MonoBehaviour
{
    public int index;

    private Vector3 originalPos;
    private SpriteRenderer sr;
    public bool isStacked = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        originalPos = transform.position;
    }

    //void OnMouseDown()
    //{
    //    if (isStacked) return;

    //    Minigame_2_11 minigame = FindObjectOfType<Minigame_2_11>();
    //    if (minigame != null)
    //        minigame.OnPlayerInput("Input");

    //    Fork_2_11 fork = FindObjectOfType<Fork_2_11>();
    //    if (fork != null)
    //        fork.GrabMacaron(gameObject);
    //}
}
