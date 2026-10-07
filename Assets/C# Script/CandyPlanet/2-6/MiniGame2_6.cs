using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGame2_6 : MiniGameBase, IPracticeDemoInput
{
    protected override float TimerDuration => 5f;
    protected override string MinigameTitle => "장애물을 피해라";

    protected override string MinigameExplain => "바움쿠헨이 계속 굴러갑니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "장애물을 피해 좌우로 움직이세요."
    };

    protected override bool UseRhythmJudgementScore => false;
    protected override int ManualTotalNodeCount => -1;

    private bool ended;

    private bool isDemoMode = false;

    [Header("References")]
    [SerializeField] private EnemySpawner2_6 spawner;
    [SerializeField] private Bawmquhen2_6 bawmquhen;


    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;

        Debug.Log(
            $"[MiniGame2_6] Demo Mode = {isDemoMode}"
        );

        if (bawmquhen != null)
        {
            bawmquhen.SetDemoMode(isDemo);
        }
        else
        {
            Debug.LogWarning(
                "[MiniGame2_6] Bawmquhen2_6이 연결되지 않았습니다."
            );
        }
    }


    public override void StartGame()
    {
        base.StartGame();

        ended = false;
    }

    public override void OnRhythmEvent(string action)
    {
        if (ended)
            return;

        if (string.IsNullOrEmpty(action))
            return;

        action = action.Trim();

        Debug.Log(
            $"[MiniGame2_6] 리듬 이벤트 = {action}"
        );

        switch (action)
        {
            case "Show":
                if (spawner == null)
                {
                    Debug.LogError(
                        "[MiniGame2_6] EnemySpawner2_6이 연결되지 않았습니다."
                    );

                    return;
                }

                int safeLane = spawner.SpawnObstacle();

                Debug.Log(
                    $"[MiniGame2_6] 장애물 생성 완료 / 안전 레인 = {safeLane}"
                );

                if (isDemoMode)
                {
                    AutoPlayTick();
                }

                break;
        }
    }


    private void AutoPlayTick()
    {
        if (ended)
            return;

        if (!isDemoMode)
            return;

        if (spawner == null)
            return;

        int safeLane = spawner.GetSafeLane();

        Debug.Log(
            $"[MiniGame2_6 AutoPlay] 현재 안전 레인 = {safeLane}"
        );

        TryMoveToLane(safeLane);
    }


    private void TryMoveToLane(int lane)
    {
        if (ended)
            return;

        if (bawmquhen == null)
        {
            Debug.LogError(
                "[MiniGame2_6] Bawmquhen2_6이 연결되지 않았습니다."
            );

            return;
        }

        Debug.Log(
            $"[MiniGame2_6] TryMoveToLane() / Lane = {lane} / Demo = {isDemoMode}"
        );

        // 데모
        if (isDemoMode)
        {
            bawmquhen.MoveToSafeLane(lane);
            return;
        }

        // 직접 플레이
        bawmquhen.MoveToPlayerLane(lane);
    }


    public void OnPlayerHit()
    {
        if (isDemoMode)
            return;

        if (ended)
            return;

        ReportManualFail();

        Debug.Log(
            "[MiniGame2_6] 장애물 충돌!"
        );
    }


    public void OnObstaclePassed()
    {
        if (ended)
            return;

        if (isDemoMode)
            return;

        ReportManualSuccess();
    }
}
