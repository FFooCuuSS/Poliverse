using System.Collections;
using UnityEngine;

public class Minigame_1_2 : MiniGameBase
{
    public override float perfectWindowOverride => 0.15f;
    public override float goodWindowOverride => 0.5f;
    public override float hitWindowOverride => 1f;

    protected override float TimerDuration => 15f;
    protected override string MinigameExplain => "체포해라!";

    [Header("Sequence Controller")]
    [SerializeField] private HandcuffSequenceController sequence;

    [Header("Timing")]
    [SerializeField] private float secondHandMustArriveWithin = 3.5f;
    [SerializeField] private float inputWindowSeconds = 0.3f;        
    [SerializeField] private float despawnFadeSeconds = 0.05f;       

    private bool ended;
    private int showCount;
    private float firstShowTime = -1f;

    public bool IsInputWindowOpen { get; private set; }
    private Coroutine inputDeadlineJob;

    private void Start()
    {
        StartGame();
    }

    public override void StartGame()
    {
        base.StartGame();
        ended = false;
        showCount = 0;
        firstShowTime = -1f;
        IsInputWindowOpen = false;

        if (inputDeadlineJob != null) StopCoroutine(inputDeadlineJob);
        inputDeadlineJob = null;

        // 시작하자마자 손 내려오는 건 OK 라 했으니,
        // 테스트용 자동 시작이 필요하면 sequence.AutoStartDebug 켜서 처리(아래 컨트롤러 수정 참고)
    }

    public void Succeed()
    {
        ended = true;
        IsInputWindowOpen = false;
        if (inputDeadlineJob != null) StopCoroutine(inputDeadlineJob);
        Success();
    }

    public void Failure()
    {
        ended = true;
        IsInputWindowOpen = false;
        if (inputDeadlineJob != null) StopCoroutine(inputDeadlineJob);
        Fail();
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended) return;
        action = action.Trim();

        if (action == "Show")
        {
            HandleShow();
            return;
        }

        if (action == "Input")
        {
            HandleInput();
            return;
        }
    }

    private void HandleShow()
    {
        showCount++;

        if (showCount == 1)
        {
            firstShowTime = Time.time;

            HandcuffFitChecker.ResetRound();     // static reset
            if (sequence != null) sequence.ResetForRound(); // 오브젝트/콜라이더/상태 리셋

            // 첫 손(왼손) 시작
            if (sequence != null)
                sequence.StartLeftMove(secondHandMustArriveWithin); // 일단 넉넉히(3.5초)로 내려오게
        }
        else if (showCount == 2)
        {
            float deadline = firstShowTime + secondHandMustArriveWithin;
            float remaining = Mathf.Max(0.1f, deadline - Time.time);

            if (sequence != null)
                sequence.StartRightMove(remaining);
        }
        else
        {
            // Show가 더 온다면, 다시 라운드로 보고 리셋하는 쪽이 안전
            firstShowTime = Time.time;
            showCount = 1;

            HandcuffFitChecker.ResetRound();
            if (sequence != null) sequence.ResetForRound();
            if (sequence != null) sequence.StartLeftMove(secondHandMustArriveWithin);
        }
    }

    private void HandleInput()
    {
        if (inputDeadlineJob != null) StopCoroutine(inputDeadlineJob);
        inputDeadlineJob = StartCoroutine(InputDeadlineCo());
    }

    private IEnumerator InputDeadlineCo()
    {
        IsInputWindowOpen = true;

        // 입력 윈도우 (0.3s)
        yield return new WaitForSeconds(inputWindowSeconds);

        IsInputWindowOpen = false;

        // 아직 종료 안 됐으면 실패 + 디스폰
        if (!ended)
        {
            Failure();
            if (sequence != null) sequence.DespawnAll(despawnFadeSeconds);
        }

        inputDeadlineJob = null;
    }

    public override void OnPlayerInput(string action = null)
    {
        if (IsInputLocked) return;
        base.OnPlayerInput(action);
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        if (IsInputLocked || ended) return;
        base.OnJudgement(judgement);

        if (judgement == JudgementResult.Miss)
            Failure();
        else
            Succeed();
    }
}