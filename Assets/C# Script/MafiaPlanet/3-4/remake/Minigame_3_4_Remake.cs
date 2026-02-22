using UnityEngine;

public class Minigame_3_4_Remake : MiniGameBase
{
    [Header("Refs")]
    [SerializeField] private Transform handTransform;          // hand 트랜스폼
    [SerializeField] private ChooseCardRemake chooser;      // hand에 붙은 ChooseCard 스크립트

    [Header("Counts")]
    [SerializeField] private int maxClickCount = 5;            // 총 5회 클릭 제한
    [SerializeField] private float handStopX = 6f;             // hand x가 6이면 최종판정

    // ====== 상태/카운트 ======
    private bool ended;

    private int successCnt;
    private int failCnt;
    private int usedClickCnt;

    // “현재 Input 슬롯” 상태
    private bool slotActive;               // 지금 Input 이벤트가 떠서 슬롯이 활성 상태인지
    private bool slotResolved;             // 이 슬롯을 이미 처리했는지(성공/실패 카운트 했는지)
    private bool playerTriedThisSlot;      // 이 슬롯에서 플레이어가 클릭 시도를 했는지

    // 이번 클릭이 trap이었는지 저장 (판정 결과 들어올 때 같이 평가)
    private bool pendingIsTrap;

    private void Start()
    {
        StartGame();
    }
    public override void StartGame()
    {
        Debug.Log("[3-4] StartGame called");
        base.StartGame();

        ended = false;

        successCnt = 0;
        failCnt = 0;
        usedClickCnt = 0;

        slotActive = false;
        slotResolved = false;
        playerTriedThisSlot = false;
        pendingIsTrap = false;

        // choosecard 입력 시작
        if (chooser != null) chooser.start = true;
    }

    private void Update()
    {
        if (ended) return;

        // hand 위치가 목표 x 도달하면 최종 판정
        if (handTransform != null && handTransform.position.x >= handStopX)
        {
            FinalJudge();
        }
    }

    /// <summary>
    /// RhythmManagerTest → MiniGameBase.OnRhythmEvent 로 들어오는 action
    /// CSV에서 Input 시간이 6.5,7.5,8.5,9.5,10.5 라고 했으니
    /// 그 타이밍에 "Input" action이 들어온다고 가정.
    /// </summary>
    public override void OnRhythmEvent(string action)
    {
        Debug.Log("[3-4] OnRhythmEvent: " + action);
        if (ended) return;
        if (string.IsNullOrEmpty(action)) return;

        action = action.Trim();

        if (action == "Input")
        {
            // 새 Input 슬롯 시작
            slotActive = true;
            slotResolved = false;
            playerTriedThisSlot = false;

            // (선택) Debug
            // Debug.Log("[3-4] Input slot opened");
        }
    }

    /// <summary>
    /// ChooseCard가 좌클릭했을 때 호출하는 진입점.
    /// “현재 hand와 닿아있는 카드”를 받아서
    /// - 클릭 제한/슬롯 상태 체크 후
    /// - 리듬매니저에 입력 전달(OnPlayerInput)
    /// </summary>
    public bool TrySubmitByClick(GameObject cardObj)
    {
        Debug.Log($"TrySubmitByClick slotActive={slotActive}, slotResolved={slotResolved}, used={usedClickCnt}, locked={IsInputLocked}, cardColor={(cardObj.GetComponent<CardColor>() != null)}");
        if (ended) return false;
        if (IsInputLocked) return false;

        // Input 슬롯이 열려있을 때만 시도 가능
        if (!slotActive) return false;

        // 이미 처리한 슬롯이면 무시(중복 카운트 방지)
        if (slotResolved) return false;

        // 총 클릭 횟수 제한
        if (usedClickCnt >= maxClickCount) return false;

        // 카드 정보 확인
        var cc = cardObj.GetComponent<CardColor>();
        if (cc == null) return false;

        usedClickCnt++;
        playerTriedThisSlot = true;
        pendingIsTrap = cc.isTrapCard;

        // 리듬매니저에 "Input" 전달 → 판정(Perfect/Good/Miss)은 매니저가 함
        OnPlayerInput("Input");

        return true;
    }

    /// <summary>
    /// RhythmManagerTest → OnPlayerJudged 로 들어오는 판정 결과
    /// - 클릭으로 들어온 판정이면: trap + Good 이상이면 success++, 아니면 fail++
    /// - 클릭 안 했는데 Miss가 들어오면(자동 미스): fail++
    /// 단, "같은 슬롯에서 클릭 미스 후 자동 미스가 한 번 더 들어오는" 중복을 막기 위해
    /// slotResolved로 한번만 카운트.
    /// </summary>
    public override void OnJudgement(JudgementResult judgement)
    {
        if (ended) return;

        // 슬롯이 활성 상태가 아니면 무시(다른 노트/이벤트로 온 판정일 가능성)
        if (!slotActive) return;

        // 이미 이 슬롯 처리 끝났으면(성공/실패 카운트 완료) 중복 방지
        if (slotResolved) return;

        // ===== 이 슬롯은 이제 한 번만 처리하고 끝 =====
        slotResolved = true;
        slotActive = false; // 슬롯 닫기

        bool timingGoodOrBetter = (judgement == JudgementResult.Good || judgement == JudgementResult.Perfect);

        if (playerTriedThisSlot)
        {
            // 플레이어가 클릭을 했던 슬롯: 규칙 A 적용
            if (pendingIsTrap && timingGoodOrBetter)
            {
                successCnt++;
                // Debug.Log($"[3-4] SUCCESS ++ (success={successCnt})");
            }
            else
            {
                failCnt++;
                // Debug.Log($"[3-4] FAIL ++ (fail={failCnt}) trap={pendingIsTrap}, judge={judgement}");
            }
        }
        else
        {
            // 클릭 안 했는데 판정이 들어온 케이스(대부분 자동 Miss)
            // "타이밍에서 miss"로 fail++
            if (judgement == JudgementResult.Miss)
            {
                failCnt++;
                // Debug.Log($"[3-4] AUTO MISS FAIL ++ (fail={failCnt})");
            }
            else
            {
                // 이 케이스는 거의 없겠지만 안전상 처리
                failCnt++;
            }
        }

        // 다음 슬롯 대비 초기화
        playerTriedThisSlot = false;
        pendingIsTrap = false;
    }

    /// <summary>
    /// hand x가 6 도달했을 때 호출됨.
    /// failCnt != 0 이면 실패
    /// failCnt == 0 이면서 successCnt == 1 이면 성공
    /// </summary>
    public void FinalJudge()
    {
        if (ended) return;
        ended = true;

        if (chooser != null) chooser.start = false;

        Debug.Log($"[3-4] FinalJudge: success={successCnt}, fail={failCnt}, usedClick={usedClickCnt}");

        // 너가 말한 조건 그대로
        if (failCnt != 0)
        {
            Fail();
            return;
        }

        if (failCnt == 0 && successCnt == 1)
        {
            Success();
            return;
        }

        // 그 외 케이스(예: fail 0인데 success 0이거나, success 2 이상 등)는 실패 처리로 두는게 안전
        Fail();
    }
}