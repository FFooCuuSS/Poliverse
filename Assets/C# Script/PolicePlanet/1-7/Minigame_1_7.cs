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

    private List<bool> holdResults = new List<bool>();

    private PrisonerController1_7 prisoner;

    private int holdSuccessCount = 0;         // 성공한 노드 수

    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.4f;
    public override float hitWindowOverride => 0.8f;

    protected override float TimerDuration => 10f;
    protected override string MinigameExplain => "금지야!";

    //private bool ended;
    private bool inputOpen;          // Input 구간인지
    private bool awaitingJudge;      // 입력 후 판정 대기중(중복 입력 방지용)

    public override void StartGame()
    {
        base.StartGame();
        holdResults.Clear();

        // 죄수 캐싱
        prisoner = FindObjectOfType<PrisonerController1_7>();
        if (prisoner != null && holdJudge != null)
        {
            holdJudge.StartAllHolds(prisoner.transform);
        }
    }

    private void Start()
    {
        // Hold UI 노드 시작
        prisoner = FindObjectOfType<PrisonerController1_7>();
        if (prisoner != null)
        {
            holdJudge.StartAllHolds(prisoner.transform);
        }
    }

    public override void OnRhythmEvent(string action)
    {
        //if (ended) return;
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

    public override void OnJudgement(JudgementResult judgement)
    {
        if (!inputOpen) return;

        inputOpen = false;
        awaitingJudge = false;

        //holdJudge.ResolveAndProceed();

        switch (judgement)
        {
            case JudgementResult.Perfect:
            case JudgementResult.Good:
                RegisterHoldSuccess();
                break;
            case JudgementResult.Miss:
                RegisterHoldFail();
                break;
        }
    }

    private void HandleHold()
    {
        if (prisoner == null) return;
        if (holdJudge.CanHold())
        {
            //holdJudge.OnHoldStart(prisoner.transform);
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

            if (holdSuccessCount >= 1/*>= maxHoldCount*/)
            {
                if (prisoner != null)
                {
                    GiveRandomProhibitToPrisoner();
                }

                Success();
            }
            else
            {
                Fail();
            }
        }
    }

    public void OnHoldButtonPressed()
    {
        if (!inputOpen || awaitingJudge) return;

        awaitingJudge = true;
        OnPlayerInput("Hold");
    }

    public void RecordHoldResult(bool success)
    {
        holdResults.Add(success);

        // 모든 UI 종료 시 최종 판정
        if (holdResults.Count >= maxHoldCount)
        {
            bool anySuccess = holdResults.Contains(true);

            if (anySuccess)
            {
                GiveRandomProhibitToPrisoner();
                Success();
            }
            else
            {
                Fail();
            }
        }
    }

    private void GiveRandomProhibitToPrisoner()
    {
        if (prisoner == null || basket == null) return;
        prisoner.DropToBasket(basket);
        Debug.Log("금지물품이 바구니로 날아갑니다!");
    }
}
