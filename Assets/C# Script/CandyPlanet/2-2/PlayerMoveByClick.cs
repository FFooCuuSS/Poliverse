using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MiniGameBase;

public class PlayerMoveByClick : MonoBehaviour
{
    [SerializeField] private float moveX = 1.5f;
    [SerializeField] private float speed = 5f;

    private Vector3 targetPos;

    public bool isMoving = false;
    public bool canMove = false;

    private bool hasMovedThisCycle = false;

    private MiniGame2_2 minigame;

    private void Start() => minigame = GetComponentInParent<MiniGame2_2>();

    void OnEnable()
    {
        Icicle.OnMoveBlocked += () => { canMove = false; };
        Icicle.OnMoveAllowed += () => { canMove = true; hasMovedThisCycle = false; };
    }
    void OnDisable()
    {
        Icicle.OnMoveBlocked -= () => { canMove = false; };
        Icicle.OnMoveAllowed -= () => { canMove = true; hasMovedThisCycle = false; };
    }
    void Update()
    {
        // 클릭 감지 및 이동 가능 여부 확인
        if (Input.GetMouseButtonDown(0) && !isMoving && canMove && !hasMovedThisCycle)
        {
            ExecuteMove(true);
        }

        // 실제 이동 로직 추가
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPos) < 0.01f)
            {
                isMoving = false; // 이동 완료 후 다시 클릭 가능 상태로 전환
            }
        }
    }

    private void ExecuteMove(bool isSuccess)
    {
        targetPos = transform.position + Vector3.right * moveX;
        isMoving = true;
        hasMovedThisCycle = true;
        canMove = false;

        if (isSuccess) minigame.OnPlayerInput("Input");
        else minigame.OnJudgement(JudgementResult.Miss);
    }

    public void ForceMove()
    {
        // 이미 이동 중이거나 입력 잠금 상태여도 강제로 다음 위치로 보냄
        targetPos = transform.position + Vector3.right * moveX;
        isMoving = true;
        canMove = false;
        hasMovedThisCycle = true; // 이동 로직을 수행하도록 설정
    }
}