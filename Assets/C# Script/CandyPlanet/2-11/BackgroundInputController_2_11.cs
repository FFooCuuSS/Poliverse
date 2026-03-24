using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundInputController_2_11 : MonoBehaviour
{
    private Minigame_2_11 minigame;
    private Fork_2_11 fork;
    private int macaronLayer;

    void Start()
    {
        minigame = FindObjectOfType<Minigame_2_11>();
        fork = FindObjectOfType<Fork_2_11>();

        macaronLayer = LayerMask.GetMask("Macaron");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("클릭 감지됨");

            // 리듬 입력 전달
            minigame?.OnPlayerInput("Input");

            // 클릭 위치 → 월드 좌표
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // 이제 마카롱 직접 클릭 안 해도 됨
            GrabMacaronBelowFork(worldPos);
        }
    }

    void GrabMacaronBelowFork(Vector2 clickWorldPos)
    {
        if (fork == null) return;

        // 포크 현재 위치 기준으로 마카롱 찾기
        float checkRadius = 0.5f; // 포크 밑으로 내려갈 범위
        Collider2D hit = Physics2D.OverlapCircle(fork.transform.position + Vector3.down * fork.moveDistance, checkRadius, macaronLayer);

        GameObject targetMacaron = hit?.gameObject;

        fork.GrabMacaron(targetMacaron, fork.transform.position);
    }

}
