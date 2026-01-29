using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReachChecker : MonoBehaviour
{
    public GameObject stage_1_3;
    private Minigame_1_3 minigame_1_3;
    private MiniGameBase minigameBase;

    private CapsuleCollider2D playerCollider;
    private DragAndDrop dragAndDrop;

    SpriteRenderer sr;
    public Sprite PlayerFail;

    private bool isGameOver = false;
    private Vector3 fixedPosition;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }
    private void Start()
    {
        minigame_1_3 = stage_1_3.GetComponent<Minigame_1_3>();
        playerCollider = GetComponent<CapsuleCollider2D>();
        dragAndDrop = GetComponent<DragAndDrop>();
        minigameBase = GetComponentInParent<MiniGameBase>();

        Vector3 pos = transform.position;
        pos.z = 0f;
        transform.position = pos;
    }

    private void Update()
    {
        if (isGameOver)
        {
            transform.position = fixedPosition; // 위치 고정
            return;
        }

        BoundCheck();
    }

    private void BoundCheck()
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            playerCollider.bounds.center,
            playerCollider.bounds.size,
            0f
        );

        bool isOnPath = false;

        foreach (var hit in hits)
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
            fixedPosition = transform.position;
            sr.sprite = PlayerFail;
            minigameBase.Fail();
        }
        
    }
    
}
