using UnityEngine;
using DG.Tweening;

public class Minigame_1_3 : MiniGameBase
{
    protected override float TimerDuration => 18f;
    protected override string MinigameExplain => "조심해라!";

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private float backDistance = 0.4f;
    [SerializeField] private float backDuration = 0.15f;
    [SerializeField] private float dashDistance = 8f;
    [SerializeField] private float dashDuration = 0.35f;

    private float currentTime;
    private bool isTimerRunning;
    private float fixedY;
    private bool lockY;

    private void Start()
    {
        //StartGame();
    }

    public override void StartGame()
    {
        base.StartGame();
        currentTime = TimerDuration;
        isTimerRunning = true;
    }

    private void Update()
    {
        RunTimer();
    }

    private void RunTimer()
    {
        if (!isTimerRunning || IsSuccess) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            isTimerRunning = false;
            //Success();
        }
    }

    public override void OnRhythmEvent(string action)
    {
        base.OnRhythmEvent(action);

        if (action == "End")
        {
            PlayEndAnimation();
        }
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        base.OnJudgement(judgement);
        if (judgement == JudgementResult.Miss)
        {
            return;
        }
    }

    // =========================
    // End 연출
    // =========================
    private void PlayEndAnimation()
    {
        if (player == null) return;

        Vector3 forward = Vector3.right;
        Vector3 startPos = player.position;

        // 튜닝값 (원하면 SerializeField로 빼도 됨)
        float backDist = backDistance;
        float dashDist = 20f;         
        float overshoot = 0.6f;       

        player.DOKill();

        Sequence seq = DOTween.Sequence();

        // 1) 뒤로: 부드러운 종료(천천히 멈추는 느낌)
        seq.Append(player.DOMove(startPos - forward * backDist, backDuration)
            .SetEase(Ease.OutSine));

        // 2) 앞으로: 부드러운 시작(가속 시작)
        seq.Append(player.DOMove(startPos + forward * dashDist, dashDuration)
            .SetEase(Ease.InSine));

        // 3) 카툰식 마무리(선택): 살짝 더 갔다가 멈추기
        seq.Append(player.DOMove(startPos + forward * (dashDist + overshoot), 0.06f)
            .SetEase(Ease.OutQuad));

        seq.OnComplete(() =>
        {
            fixedY = player.position.y;
            lockY = true;
        });

        Success();
    }
}
