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

    // 접시에 쌓여있던 마카롱들을 전부 삭제하고 리스트를 비운다.
    // 라운드가 끝날 때 호출해서 다음 라운드를 위해 접시를 깨끗하게 초기화한다.
    public void ClearPlate()
    {
        for (int i = 0; i < stackedMacarons.Count; i++)
        {
            if (stackedMacarons[i] != null)
                Destroy(stackedMacarons[i].gameObject);
        }

        stackedMacarons.Clear();
    }
}
