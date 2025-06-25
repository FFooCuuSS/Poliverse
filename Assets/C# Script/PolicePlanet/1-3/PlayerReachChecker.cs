using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReachChecker : MonoBehaviour
{
    public GameObject stage_1_3;
    private Minigame_1_3 minigame_1_3;

    [SerializeField] private BoxCollider2D goalCollider;
    private CapsuleCollider playerCollider;
    private DragAndDrop dragAndDrop;

    private bool isGameOver = false;
    private Vector3 fixedPosition;

    private void Start()
    {
        minigame_1_3 = stage_1_3.GetComponent<Minigame_1_3>();
        playerCollider = GetComponent<CapsuleCollider>();
        dragAndDrop = GetComponent<DragAndDrop>();

        Vector3 pos = transform.position;
        pos.z = 0f;
        transform.position = pos;
    }

    private void Update()
    {
        if (isGameOver)
        {
            transform.position = fixedPosition; // ��ġ ����
            return;
        }

        BoundCheck();
        GoalCheck();
    }

    private void OnMouseDown()
    {
        Debug.Log("������");
    }

    private void BoundCheck()
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(transform.position);

        bool isOnPath = false;

        foreach(var hit in hits)
        {
            if (hit.CompareTag("Path"))
            {
                isOnPath = true;
                break;
            }
        }
        if (!isOnPath && !isGameOver)
        {
            isGameOver = true;
            fixedPosition = transform.position; // ���� ��ġ ����
            minigame_1_3.Failure();
        }
    }

    private void GoalCheck()
    {
        Bounds goalBounds = goalCollider.bounds;
        Bounds playerBounds = playerCollider.bounds;

        if (playerBounds.Intersects(goalBounds) && !isGameOver)
        {
            isGameOver = true;
            fixedPosition = transform.position; // ���� ��ġ ����
            minigame_1_3.Succeed();
        }
    }
}
