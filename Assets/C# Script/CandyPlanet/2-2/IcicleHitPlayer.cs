using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class IcicleHitPlayer : MonoBehaviour
{
    private MiniGame2_2 minigame_2_2;
    public GameObject stage_2_2;
    public PlayerMoveByClick playerMove;

    private void Start()
    {
        minigame_2_2 = stage_2_2.GetComponent<MiniGame2_2>();
    }
    private void OnTriggerEnter2D(Collider2D coll) 
    {
        // 태그가 Icicle인 것과 충돌했는지 확인
        if (coll.CompareTag("Icicle"))
        {
            Debug.Log("충돌 감지 성공!");

            minigame_2_2.missCount++;
            minigame_2_2.CheckGameResult();

            // 플레이어 본인의 이동 스크립트 호출
            var playerMove = GetComponent<PlayerMoveByClick>();
            if (playerMove != null)
            {
                playerMove.ForceMove();
            }
        }
    }


    // 제한시간 추가
}