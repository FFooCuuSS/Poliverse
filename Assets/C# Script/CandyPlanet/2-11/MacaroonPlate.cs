using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MacaroonPlate : MonoBehaviour
{
    public float yOffset = 0.4f;
    private List<Macaron> stackedMacarons = new List<Macaron>();

    public Minigame_2_11 minigame;

    public void AddMacaron(Macaron macaron)
    {
        stackedMacarons.Add(macaron);

        int order = stackedMacarons.Count;

        macaron.transform.SetParent(transform);
        macaron.transform.localPosition = new Vector3(0, yOffset * (order - 1), 0);

        SpriteRenderer sr = macaron.GetComponent<SpriteRenderer>();

        sr.sortingLayerName = "Macaron";   // 마카롱 레이어
        sr.sortingOrder = order;           // 쌓인 순서대로 증가
    }

}
