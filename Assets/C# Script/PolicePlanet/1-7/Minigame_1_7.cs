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

    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.4f;
    public override float hitWindowOverride => 0.8f;

    protected override float TimerDuration => 10f;
    protected override string MinigameExplain => "금지야!";

    private bool ended;
    private bool inputOpen;          // Input 구간인지
    private bool awaitingJudge;      // 입력 후 판정 대기중(중복 입력 방지용)

    void Update()
    {
        // 테스트용 입력
        holdInputThisFrame = Input.GetKeyDown(KeyCode.Space);
    }

    public override void StartGame()
    {
        base.StartGame();
        holdSuccessCount = 0;
    }

    private void Start()
    {
        // Hold UI 노드 시작
        PrisonerController1_7 prisoner = FindObjectOfType<PrisonerController1_7>();
        if (prisoner != null)
        {
            holdJudge.StartAllHolds(prisoner.transform);
        }
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended) return;
        Debug.Log($"{gameObject.name} 리듬메세지: {action}");
        action = action.Trim();

        if (action == "Hold")
        {
            inputOpen = true;
            awaitingJudge = false;

            HandleHold();
        }
    }

    public override void OnPlayerInput(string action = null)
    {
        // 입력 잠금 상태면 무시
        if (IsInputLocked) return;
        base.OnPlayerInput(action);
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

            if (holdSuccessCount <= maxHoldCount)
            {
                if (prisoner != null)
                {
                    //GiveRandomProhibitToPrisoner(prisoner);
                }

                GiveRandomProhibitToPrisoner(prisoner);

                Success();
            }
            //else
            //{
            //    Fail();
            //}
        }
    }

    public bool WasHoldInputPressed()
    {
        return holdInputThisFrame;
    }

    private void GiveRandomProhibitToPrisoner(PrisonerController1_7 prisoner)
    {
        prisoner.DropToBasket(basket);
        Debug.Log("금지물품이 바구니로 날아갑니다!");
    }

}
