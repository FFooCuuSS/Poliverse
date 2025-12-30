using System.Collections.Generic;
using UnityEngine;

public class HandcuffFitChecker : MonoBehaviour
{
    public GameObject stage_1_2;
    private Minigame_1_2 minigame_1_2;
    private MiniGameBase minigameBase;

    [SerializeField] private GameObject objectToDestroy_1;
    [SerializeField] private GameObject objectToDestroy_2;

    [SerializeField] private GameObject objectToSpawn;


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
        minigameBase = GetComponentInParent<MiniGameBase>();
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
            if (objectToDestroy_1 != null)    objectToDestroy_1.SetActive(false);
            if (objectToDestroy_2 != null)    objectToDestroy_2.SetActive(false);
            if (objectToSpawn != null)        objectToSpawn.SetActive(true);

            //minigame_1_2.Succeed();
            minigameBase.OnPlayerInput();
            minigameBase.Success();
        }
        else
        {
            minigame_1_2.Fail();
        }
    }
    public void ForceSnapToHand(CircleCollider2D handCol)
    {
        transform.position = handCol.bounds.center;
        isSnapped = true;
        snappedHand = handCol;

        if (dragAndDrop != null)
            dragAndDrop.enabled = false;

        GameClearCheck();
    }
}
