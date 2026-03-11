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

    [Header("게임 설정")]
    public int totalRound = 3;     // 총 라운드
    public int holdPerRound = 3;  // 라운드당 Hold 수

    private int currentRound = 0;
    private int holdIndex = 0;
    private int totalSuccess = 0;

    private PrisonerController1_7 prisoner;

    [SerializeField] private PrisonerSpawner1_7 prisonerSpawner;

    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.4f;
    public override float hitWindowOverride => 0.8f;

    protected override float TimerDuration => 10f;
    protected override string MinigameExplain => "금지야!";

    private bool inputOpen;          // Input 구간인지
    private bool awaitingJudge;      // 입력 후 판정 대기중(중복 입력 방지용)

    public void SetPrisoner(PrisonerController1_7 p)
    {
        prisoner = p;

        StartRound();
    }

    public override void StartGame()
    {
        base.StartGame();

        currentRound = 0;
        holdIndex = 0;
        totalSuccess = 0;

        if (holdJudge != null)
        {
            holdJudge.minigame = this;
        }
    }

    private void StartRound()
    {
        Debug.Log("StartRound 실행 : " + (currentRound + 1));

        holdIndex = 0;

        if (prisoner != null && holdJudge != null)
        {
            holdJudge.ResetHoldNodes();
            holdJudge.StartAllHolds(prisoner.transform);
        }
    }

    public override void OnRhythmEvent(string action)
    {
        action = action.Trim();
        Debug.Log($"{gameObject.name} 리듬메세지: {action}");

        if (action == "Hold")
        {
            inputOpen = true;
            awaitingJudge = false;
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


        switch (judgement)
        {
            case JudgementResult.Perfect:
            case JudgementResult.Good:
                totalSuccess++;
                break;

            case JudgementResult.Miss:
                break;
        }

        holdIndex++;

        CheckRoundEnd();
    }

    private void CheckRoundEnd()
    {
        if (holdIndex < holdPerRound)
            return;

        // 한 라운드 홀드 모두 끝남
        // 아이템 떨어뜨리고 범인 제거
        if (prisoner != null && prisoner.GetProhibitedItem() != null)
        {
            prisoner.DropToBasket(basket);
        }

        Destroy(prisoner.gameObject);
        prisoner = null;

        currentRound++;

        Debug.Log("Round End");

        if (currentRound >= totalRound)
        {
            EndGame();
        }
        else
        {
            SpawnNewPrisonerAndStartRound();
        }
    }

    private void SpawnNewPrisonerAndStartRound()
    {
        if (prisonerSpawner == null)
        {
            Debug.LogError("PrisonerSpawner가 할당되어 있지 않습니다.");
            return;
        }

        // Minigame_1_7 오브젝트를 부모로 지정
        GameObject newPrisonerObj = prisonerSpawner.SpawnRandomPrisoner(this.transform);
        if (newPrisonerObj == null)
        {
            Debug.LogError("새로운 죄수 생성 실패");
            return;
        }

        prisoner = newPrisonerObj.GetComponent<PrisonerController1_7>();
        if (prisoner == null)
        {
            Debug.LogError("PrisonerController1_7 컴포넌트가 새로 생성된 죄수에 없음");
            return;
        }

        StartRound();
    }


    private void EndGame()
    {
        Debug.Log("Game End. Success count = " + totalSuccess);

        if (totalSuccess >= 3)
        {
            GameManager1_7.instance.OnMinigameSuccess(basket);
            Success();
        }
        else
        {
            Fail();
        }
    }

    public void OnHoldButtonPressed()
    {
        if (!inputOpen || awaitingJudge) return;

        awaitingJudge = true;
        OnPlayerInput("Hold");
    }

    private void GiveRandomProhibitToPrisoner()
    {
        if (prisoner == null || basket == null) return;
        prisoner.DropToBasket(basket);
        Debug.Log("금지물품이 바구니로 날아갑니다!");
    }
}
