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

        int count = stackedMacarons.Count;

        macaron.transform.SetParent(transform);

        macaron.transform.localPosition = new Vector3(0, yOffset * (count - 1), 0);

        SpriteRenderer sr = macaron.GetComponent<SpriteRenderer>();


        if (sr != null)
        {
            sr.sortingLayerName = "Macaron";

            sr.sortingOrder = count;
        }
    }
}
