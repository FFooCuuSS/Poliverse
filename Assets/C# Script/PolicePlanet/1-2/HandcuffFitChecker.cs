using System.Collections.Generic;
using UnityEngine;

public class HandcuffFitChecker : MonoBehaviour
{
    public GameObject stage_1_2;
    private Minigame_1_2 minigame_1_2;

    [SerializeField] private List<CircleCollider2D> handColliders;
    [SerializeField] private List<DragAndDrop> dragAndDrops;
    private CircleCollider2D cuffCollider;
    private DragAndDrop dragAndDrop;

    private bool isSnapped = false;
    private CircleCollider2D snappedHand = null;
    private static bool isGameEnded = false;

    private void Start()
    {
        minigame_1_2 = stage_1_2.GetComponent<Minigame_1_2>();
        cuffCollider = GetComponent<CircleCollider2D>();
        dragAndDrop = GetComponent<DragAndDrop>();
    }

    private void Update()
    {
        if (!isSnapped && !isGameEnded)
        {
            CheckAndSnap();
        }
    }

    private void CheckAndSnap()
    {
        foreach (var handcol in handColliders)
        {
            if (cuffCollider.bounds.Intersects(handcol.bounds))
            {
                transform.position = handcol.bounds.center;
                isSnapped = true;
                snappedHand = handcol;

                if (dragAndDrop != null) dragAndDrop.enabled = false;

                Debug.Log($"수갑 {gameObject.name} 겹침, 고정 완료");

                GameClearCheck();
                break;
            }
        }
    }

    private void GameClearCheck()
    {
        if (isGameEnded) return;

        var all = FindObjectsOfType<HandcuffFitChecker>();

        foreach (var cuff in all)
        {
            if (!cuff.isSnapped)
                return;
        }

        isGameEnded = true;

        if (all[0].snappedHand != all[1].snappedHand)
        {
            minigame_1_2.Succeed();
        }
        else
        {
            minigame_1_2.Fail();
        }
    }
}
