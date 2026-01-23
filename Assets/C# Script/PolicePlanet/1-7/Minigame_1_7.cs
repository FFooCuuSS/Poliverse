using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_1_7 : MiniGameBase
{
    [Header("금지물품 Prefabs & Basket")]
    public GameObject[] ProhibitPrefabs;      // 여러 금지물품 Prefab
    public Transform basket;                  // 떨어질 위치 (선택적)

    [Header("Hold System")]
    public HoldCheck1_7 holdJudge;
    public int maxHoldCount = 4;              // 전체 UI 노드 수

    private int holdSuccessCount = 0;         // 성공한 노드 수
    private bool holdInputThisFrame = false;

    protected override float TimerDuration => 10f;
    protected override string MinigameExplain => "금지야!";

    void Update()
    {
        // 테스트용 입력
        holdInputThisFrame = Input.GetKeyDown(KeyCode.Space);
    }

    public override void StartGame()
    {
        holdSuccessCount = 0;

        // Hold UI 노드 시작
        PrisonerController1_7 prisoner = FindObjectOfType<PrisonerController1_7>();
        if (prisoner != null)
        {
            holdJudge.StartAllHolds(prisoner.transform);
        }
    }

    public override void OnRhythmEvent(string action)
    {
        switch (action)
        {
            case "Tap":
                Debug.Log("Tap");
                break;
            case "Hold":
                HandleHold();
                break;
            case "Swipe":
                Debug.Log("Swipe");
                break;
        }
    }

    private void HandleHold()
    {
        PrisonerController1_7 prisoner = FindObjectOfType<PrisonerController1_7>();
        if (prisoner == null) return;

        if (holdJudge.CanHold())
        {
            holdJudge.OnHoldStart(prisoner.transform);
        }
    }

    public void RegisterHoldSuccess()
    {
        holdSuccessCount++;
        CheckMinigameEnd();
    }

    public void RegisterHoldFail()
    {
        CheckMinigameEnd();
    }

    private void CheckMinigameEnd()
    {
        if (holdJudge == null) return;

        if (holdJudge.CurrentUINode >= maxHoldCount)
        {
            PrisonerController1_7 prisoner = FindObjectOfType<PrisonerController1_7>();

            if (holdSuccessCount >= maxHoldCount)
            {
                Debug.Log("[Minigame_1_7] 미니게임 성공!");

                if (prisoner != null)
                {
                    // 금지물품 랜덤 생성 + 범인 손에 장착
                    GiveRandomProhibitToPrisoner(prisoner);
                }

                Success();
            }
            else
            {
                Debug.Log("[Minigame_1_7] 미니게임 실패!");
                Fail();
            }
        }
    }

    public bool WasHoldInputPressed()
    {
        return holdInputThisFrame;
    }

    private void GiveRandomProhibitToPrisoner(PrisonerController1_7 prisoner)
    {
        if (ProhibitPrefabs.Length == 0)
        {
            Debug.LogWarning("금지물품 Prefab이 없습니다!");
            return;
        }

        // 금지물품을 바구니로 떨어뜨리기
        prisoner.DropToBasket(basket);

        Debug.Log("금지물품이 나왔습니다!");
    }

}
