using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MacaroonPlate : MonoBehaviour
{
    public float yOffset = 0.4f;
    private List<Macaron> stackedMacarons = new List<Macaron>();

    public Minigame_2_11 minigame;

    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sortingOrder = -1;
    }

    public void AddMacaron(Macaron macaron)
    {
        stackedMacarons.Add(macaron);

        int order = stackedMacarons.Count;
        macaron.SetOrder(order);

        macaron.transform.SetParent(transform);
        macaron.transform.localPosition = new Vector3(0, yOffset * (order - 1), 0);

        if (stackedMacarons.Count == 5)
            CheckAnswer();
    }

    private void CheckAnswer()
    {
        CompletedMacaroon answer = FindObjectOfType<CompletedMacaroon>();

        for (int i = 0; i < 5; i++)
        {
            if (stackedMacarons[i].index != answer.answerOrder[i])
            {
                minigame.Failure();
                return;
            }
        }

        minigame.Succeed();
    }
}
