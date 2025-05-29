using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class HandcuffFitChecker : MonoBehaviour
{
    [SerializeField] private List<CircleCollider2D> handColliders;
    [SerializeField] private List<DragAndDrop> dragAndDrops;
    private CircleCollider2D cuffCollider;
    private DragAndDrop dragAndDrop;

    private bool isSnapped = false;


    private void Start()
    {
        cuffCollider = GetComponent<CircleCollider2D>();
        dragAndDrop = GetComponent<DragAndDrop>();
    }
    private void Update()
    {
        if (!isSnapped)
        {
            CheckAndSnap();
        }
        GameClearCheck();
    }

    private void CheckAndSnap()
    {
        foreach (var handcol in handColliders)
        {
            if (cuffCollider.bounds.Intersects(handcol.bounds))
            {
                transform.position = handcol.bounds.center;
                isSnapped = true;

                if (dragAndDrop != null) dragAndDrop.enabled = false;
                Debug.Log($"수갑 {gameObject.name} 겹침, 고정 완료"); break;
            }
        }
    }

    private void GameClearCheck()
    {
        var all = FindObjectsOfType<HandcuffFitChecker>();
        foreach (var cuff in all)
        {
            if (!cuff.isSnapped) return;
        }
        Debug.Log("gameClear");
    }
}
